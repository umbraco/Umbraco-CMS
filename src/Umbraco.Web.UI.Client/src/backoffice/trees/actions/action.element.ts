import { LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction, ManifestTree } from '../../../core/models';
import { Entity } from '../../../mocks/data/entity.data';
import { UmbSectionContext } from '../../sections/section.context';
import { UmbActionPageService } from './action-page.service';
import { UmbTreeContextMenuService } from '../shared/context-menu/tree-context-menu.service';

export type ActionPageEntity = {
	key: string;
	name: string;
};

@customElement('umb-action')
export default class UmbActionElement extends UmbContextConsumerMixin(LitElement) {
	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	@state()
	protected _entity: ActionPageEntity = { name: '', key: '' };

	protected _activeTree?: ManifestTree;
	protected _activeTreeItem?: Entity;

	protected _sectionContext?: UmbSectionContext;
	protected _treeContextMenuService?: UmbTreeContextMenuService;
	protected _actionPageService?: UmbActionPageService;

	protected _actionPageSubscription?: Subscription;
	protected _activeTreeSubscription?: Subscription;
	protected _activeTreeItemSubscription?: Subscription;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext('umbSectionContext', (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeActiveTree();
			this._observeActiveTreeItem();
		});

		this.consumeContext('umbTreeContextMenuService', (treeContextMenuService: UmbTreeContextMenuService) => {
			this._treeContextMenuService = treeContextMenuService;
		});

		this.consumeContext('umbActionPageService', (actionPageService: UmbActionPageService) => {
			this._actionPageService = actionPageService;

			this._actionPageSubscription?.unsubscribe();
			this._actionPageService?.entity.subscribe((entity: ActionPageEntity) => {
				this._entity = entity;
			});
		});
	}

	private _observeActiveTree() {
		this._activeTreeSubscription?.unsubscribe();

		this._activeTreeSubscription = this._sectionContext?.activeTree.subscribe((tree) => {
			this._activeTree = tree;
		});
	}

	private _observeActiveTreeItem() {
		this._activeTreeItemSubscription?.unsubscribe();

		this._activeTreeItemSubscription = this._sectionContext?.activeTreeItem.subscribe((treeItem) => {
			this._activeTreeItem = treeItem;
		});
	}

	disconnectCallback() {
		super.disconnectedCallback();
		this._actionPageSubscription?.unsubscribe();
		this._activeTreeSubscription?.unsubscribe();
		this._activeTreeItemSubscription?.unsubscribe();
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-action': UmbActionElement;
	}
}
