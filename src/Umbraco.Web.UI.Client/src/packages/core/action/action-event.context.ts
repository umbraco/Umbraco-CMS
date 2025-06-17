import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbActionEventContext extends UmbContextBase {
	/* It is not possible to add native event listeners to all events on a event target.
	This is a workaround to be able to listen to all events.
	It could potentially replace the need for the native events if we want to. */
	public readonly events = new Subject<Event>();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ACTION_EVENT_CONTEXT);
	}

	/* Override the native dispatchEvent method to also push the event to the subject */
	override dispatchEvent(event: UmbEntityActionEvent): boolean {
		this.events.next(event);
		return super.dispatchEvent(event);
	}

	// TODO: revisit this. This is currently used to prevent a
	public localDispatchEvent(event: UmbEntityActionEvent) {
		super.dispatchEvent(event);
	}
}

export const UMB_ACTION_EVENT_CONTEXT = new UmbContextToken<UmbActionEventContext, UmbActionEventContext>(
	'UmbActionEventContext',
);
