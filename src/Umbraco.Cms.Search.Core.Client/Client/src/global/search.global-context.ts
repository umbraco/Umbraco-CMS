import { UMB_SEARCH_SERVER_EVENT_TYPE } from './constants.js';

import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '@umbraco-cms/backoffice/management-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchContext extends UmbContextBase {
  #serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
  #userWaitingForIndexUpdate = new Set<string>();
  #localize = new UmbLocalizationController(this);

  /**
   * Observable that emits the index alias when an index rebuild completes.
   * Subscribe to this instead of directly observing server events.
   */
  #indexRebuilt = new UmbBasicState<string | undefined>(undefined);
  public readonly indexRebuilt = this.#indexRebuilt.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, UMB_SEARCH_CONTEXT);

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      this.#notificationContext = instance;
    });

    this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (instance) => {
      this.#serverEventContext = instance;
      this.#observeSearchIndexChanges();
    });
  }

  setUserWaitingForIndexUpdate(indexAlias: string, isWaiting: boolean) {
    if (isWaiting) {
      this.#userWaitingForIndexUpdate.add(indexAlias);
    } else {
      this.#userWaitingForIndexUpdate.delete(indexAlias);
    }
  }

  #isUserWaitingForIndexUpdate(indexAlias: string): boolean {
    return this.#userWaitingForIndexUpdate.has(indexAlias);
  }

  #observeSearchIndexChanges() {
    this.observe(
      this.#serverEventContext?.byEventSource(UMB_SEARCH_SERVER_EVENT_TYPE),
      (args) => {
        if (!args?.eventSource) return;

        const indexAlias = String(args.eventSource);

        // Emit on the public observable for subscribers
        this.#indexRebuilt.setValue(indexAlias);

        // Handle user notification if they were waiting
        if (this.#isUserWaitingForIndexUpdate(indexAlias)) {
          this.setUserWaitingForIndexUpdate(indexAlias, false);

          this.#notificationContext?.peek('positive', {
            data: {
              title: this.#localize.term('search_rebuildCompletedTitle'),
              message: this.#localize.term('search_rebuildCompletedMessage', indexAlias),
            },
          });
        }
      },
      'index-rebuild-notification-observer',
    );
  }
}

export const UMB_SEARCH_CONTEXT = new UmbContextToken<UmbSearchContext>('UmbSearchContext');
