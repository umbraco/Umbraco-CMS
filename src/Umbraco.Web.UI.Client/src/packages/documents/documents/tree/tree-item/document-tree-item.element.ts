import type { UmbDocumentTreeItemModel } from '../types.js';
import type { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { css, html, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
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

	#icon: string | null | undefined;

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected override _extractFlags(item: UmbDocumentTreeItemModel | undefined) {
		// Empty on purpose and NOT calling super to prevent doing what the base does. [NL]
	}

	protected override _getIconName(): string | null | undefined {
		return this.#icon;
	}

	override renderLabel() {
		return html`<span id="label" slot="label">${this._name}</span> `;
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
		`,
	];
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
