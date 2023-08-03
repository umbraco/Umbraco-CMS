import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-entity-action-list')
export class UmbEntityActionListElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	private _entityType: string = '';
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		if (value === this._entityType) return;
		this._entityType = value;
		const oldValue = this._filter;
		this._filter = (extension: ManifestEntityAction) => extension.meta.entityTypes.includes(this.entityType);
		this.requestUpdate('_filter', oldValue);
	}

	@state()
	_filter?: (extension: ManifestEntityAction) => boolean;

	@property({ type: String })
	public unique?: string | null;

	render() {
		return this._filter
			? html`
					<umb-extension-slot
						type="entityAction"
						default-element="umb-entity-action"
						.filter=${this._filter}
						.props=${{ unique: this.unique }}></umb-extension-slot>
			  `
			: '';
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action-list': UmbEntityActionListElement;
	}
}
