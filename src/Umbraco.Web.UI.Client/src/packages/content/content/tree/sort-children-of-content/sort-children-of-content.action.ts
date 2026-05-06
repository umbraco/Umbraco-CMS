import { UMB_SORT_CHILDREN_OF_CONTENT_MODAL } from './constants.js';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import {
    UmbSortChildrenOfEntityAction,
    type UmbSortChildrenOfModalData,
    type UmbSortChildrenOfModalValue,
} from '@umbraco-cms/backoffice/tree';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';  

/**
 * Entity action for sorting children of a content item
 * @class UmbSortChildrenOfContentEntityAction
 * @augments UmbSortChildrenOfEntityAction
 */
export class UmbSortChildrenOfContentEntityAction extends UmbSortChildrenOfEntityAction {
    protected override _getModalToken(): UmbModalToken<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
        return UMB_SORT_CHILDREN_OF_CONTENT_MODAL;
    }

    override async execute() {
        const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
        const culture = appLanguageContext?.getAppCulture() ?? null;

        await umbOpenModal(this, this._getModalToken(), {
            data: {
                unique: this.args.unique,
                entityType: this.args.entityType,
                sortChildrenOfRepositoryAlias: this.args.meta.sortChildrenOfRepositoryAlias,
                treeRepositoryAlias: this.args.meta.treeRepositoryAlias,
                culture,
            },
        }).catch(() => undefined);

        const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
        if (!eventContext) throw new Error('Event context is not available');
        eventContext.dispatchEvent(
            new UmbRequestReloadChildrenOfEntityEvent({
                unique: this.args.unique,
                entityType: this.args.entityType,
            }),
        );
    }
}

export { UmbSortChildrenOfContentEntityAction as api };
