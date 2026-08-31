import type { UmbDocumentTreeItemModel } from '../types.js';
import type { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { css, html, customElement, state, property, classMap, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbTreeItemElementBase<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeItemContext
> {
	#api: UmbDocumentTreeItemContext | undefined;

	@property({ type: Object, attribute: false })
	public override set api(value: UmbDocumentTreeItemContext | undefined) {
		this.#api = value;

		if (this.#api) {
			this.observe(this.#api.name, (name) => (this._name = name || ''));
			this.observe(this.#api.isDraft, (isDraft) => (this._isDraft = isDraft || false));
			this.observe(this.#api.hasCollection, (has) => {
				const oldValue = this._forceShowExpand;
				this._forceShowExpand = has;
				this.requestUpdate('_forceShowExpand', oldValue);
			});
			this.observe(this.#api.drillableCollection, (drillable) => (this._drillableCollection = drillable));
			this.observe(this.#api.collapsibleCollection, (collapsible) => (this._collapsibleCollection = collapsible));
			this.observe(this.#api.icon, (icon) => (this.#icon = icon || ''));
			this.observe(this.#api.flags, (flags) => (this._flags = flags || []));
		}

		super.api = value;
	}
	public override get api(): UmbDocumentTreeItemContext | undefined {
		return this.#api;
	}

	@state()
	private _name = '';

	/**
	 * Indicates whether the document is a draft, this is controlled internally but present as an attribute as it affects styling.
	 * @internal
	 */
	@property({ type: Boolean, reflect: true, attribute: 'draft' })
	protected _isDraft = false;

	@property({ type: Boolean, reflect: true, attribute: 'collection' })
	private _collapsibleCollection = false;

	@state()
	private _drillableCollection = false;

	#icon: string | null | undefined;

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected override _extractFlags(item: UmbDocumentTreeItemModel | undefined) {
		// Empty on purpose and NOT calling super to prevent doing what the base does. [NL]
	}

	protected override _getIconName(): string | null | undefined {
		return this.#icon;
	}

	protected override _renderChildItem(item: UmbDocumentTreeItemModel) {
		if (this._collapsibleCollection) {
			return html`<div class="child">
				<div class="peek-child"></div>
				${super._renderChildItem(item)}
			</div>`;
		} else {
			return super._renderChildItem(item);
		}
	}

	// eslint-disable-next-line @typescript-eslint/naming-convention
	override _renderExpandSymbol = () => {
		// The list icon replaces the expand arrow only where activating it drills into the Collection — see
		// `drillableCollection`. Where it would do nothing, the normal caret is rendered and the children expand, so a
		// subtree is never made unreachable by an affordance that cannot act.
		if (!this._drillableCollection) return undefined;
		return html`<umb-icon data-mark="open-collection" name="icon-list" style="font-size: 8px;"></umb-icon>`;
	};

	override renderLabel() {
		return html`<span
			id="label"
			slot="label"
			class=${classMap({ draft: this._isDraft, noAccess: this._noAccess })}
			@dblclick=${this._handleDblClick}>
			${this._name}
		</span>`;
	}

	protected override _renderLoadPrevButton() {
		if (this._drillableCollection) return nothing;
		return super._renderLoadPrevButton();
	}
	protected override _renderLoadNextButton() {
		if (this._drillableCollection) return nothing;
		return super._renderLoadNextButton();
	}

	static override styles = [
		...UmbTreeItemElementBase.styles,
		css`
			:host {
				--umb-tree-item-indent: calc(var(--uui-menu-item-indent, 0) + 1);
			}

			/* A collapsible collection is peeked, not drilled into — its children stay at its own indent rather than one level deeper. */
			:host([collection]) uui-menu-item::slotted(*:not([name])) {
				--uui-menu-item-indent: var(--umb-tree-item-indent, 0);
				--uui-menu-item-flat-structure: 1;
			}
			:host([collection]) uui-menu-item {
				--uui-menu-item-child-indent: 0;
			}

			:host([draft]) #label {
				opacity: 0.6;
			}
			:host([draft]) umb-icon {
				opacity: 0.6;
			}

			.child {
				display: flex;
				flex-direction: row;
				align-items: flex-start;
			}
			umb-tree-item {
				flex-grow: 1;
				flex-shrink: 1;
			}
			.peek-child {
				position: relative;
				display: block;
				flex-grow: 0;
				flex-shrink: 0;
				width: calc((2 + var(--umb-tree-item-indent, 0)) * var(--uui-size-4));
			}
			.peek-child::after {
				content: '';
				position: absolute;
				z-index: 1;
				top: -11px;
				right: 3px;
				border-left: 1px solid var(--uui-color-border-standalone);
				border-bottom: 1px solid var(--uui-color-border-standalone);
				border-bottom-left-radius: var(--uui-border-radius);
				width: 16px;
				height: 28px;
			}
		`,
	];
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
