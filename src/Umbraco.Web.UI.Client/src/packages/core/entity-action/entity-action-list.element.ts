import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-action-list')
export class UmbEntityActionListElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
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
	private _entityType: string = '';

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
						.props=${{ unique: this.unique, entityType: this.entityType }}></umb-extension-slot>
			  `
			: '';
	}

	static styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action-list': UmbEntityActionListElement;
	}
}
