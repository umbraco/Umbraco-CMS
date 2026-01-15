import type { UmbDocumentTreeItemModel } from '../types.js';
import type { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { css, html, customElement, state, property, classMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbTreeItemElementBase<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeItemContext
> {
	#api: UmbDocumentTreeItemContext | undefined;

	@property({ type: Object, attribute: false })
	public override get api(): UmbDocumentTreeItemContext | undefined {
		return this.#api;
	}
	public override set api(value: UmbDocumentTreeItemContext | undefined) {
		this.#api = value;

		if (this.#api) {
			this.observe(this.#api.name, (name) => (this._name = name || ''));
			this.observe(this.#api.isDraft, (isDraft) => (this._isDraft = isDraft || false));
			this.observe(this.#api.noAccess, (noAccess) => (this._noAccess = noAccess || false));
			this.observe(this.#api.hasCollection, (has) => {
				const oldValue = this._forceShowExpand;
				this._forceShowExpand = has;
				this.requestUpdate('_forceShowExpand', oldValue);
			});
			this.observe(this.#api.icon, (icon) => (this.#icon = icon || ''));
			this.observe(this.#api.flags, (flags) => (this._flags = flags || ''));
		}

		super.api = value;
	}

	@state()
	private _name = '';

	/**
	 * @internal
	 * Indicates whether the document is a draft, this is controlled internally but present as an attribute as it affects styling.
	 */
	@property({ type: Boolean, reflect: true, attribute: 'draft' })
	protected _isDraft = false;

	/**
	 * @internal
	 * Indicates whether the user has no access to this document, this is controlled internally but present as an attribute as it affects styling.
	 */
	@property({ type: Boolean, reflect: true, attribute: 'no-access' })
	protected _noAccess = false;

	#icon: string | null | undefined;

	constructor() {
		super();
		this.addEventListener('click', this.#handleClick);
	}

	#handleClick = (event: MouseEvent) => {
		if (this._noAccess) {
			event.preventDefault();
			event.stopPropagation();
		}
	};

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected override _extractFlags(item: UmbDocumentTreeItemModel | undefined) {
		// Empty on purpose and NOT calling super to prevent doing what the base does. [NL]
	}

	protected override _getIconName(): string | null | undefined {
		return this.#icon;
	}

	// eslint-disable-next-line @typescript-eslint/naming-convention
	override _renderExpandSymbol = () => {
		// If this in the menu and it is a collection, then we will enforce the user to the Collection view instead of expanding.
		// `this._forceShowExpand` is equivalent to hasCollection for this element.
		if (this._isMenu && this._forceShowExpand) {
			return html`<umb-icon data-mark="open-collection" name="icon-list" style="font-size: 8px;"></umb-icon>`;
		} else {
			return undefined;
		}
	};

	override renderLabel() {
		return html`<span id="label" slot="label" class=${classMap({ draft: this._isDraft })}>${this._name}</span> `;
	}

	static override styles = [
		...UmbTreeItemElementBase.styles,
		css`
			:host([draft]) #label {
				opacity: 0.6;
			}
			:host([draft]) umb-icon {
				opacity: 0.6;
			}
			:host([no-access]) {
				cursor: not-allowed;
			}
			:host([no-access]) #label {
				opacity: 0.6;
				font-style: italic;
			}
			:host([no-access]) umb-icon {
				opacity: 0.6;
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
