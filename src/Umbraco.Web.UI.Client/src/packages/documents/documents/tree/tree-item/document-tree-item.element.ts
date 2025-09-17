import type { UmbDocumentTreeItemModel } from '../types.js';
import type { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { css, html, customElement, classMap, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
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
			this.observe(this.#api.icon, (icon) => (this._icon = icon || ''));
		}

		super.api = value;
	}

	@state() private _name = '';
	@state() private _isDraft = false;

	override renderLabel() {
		return html`<span id="label" slot="label" class=${classMap({ draft: this._isDraft })}>${this._name}</span> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#label {
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
			}

			.draft {
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
