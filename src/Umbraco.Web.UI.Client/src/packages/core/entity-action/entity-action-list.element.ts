import type { UmbEntityActionArgs } from './types.js';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestEntityAction, MetaEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-action-list')
export class UmbEntityActionListElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string | undefined {
		return this._props.entityType;
	}
	public set entityType(value: string | undefined) {
		if (value === undefined || value === this._props.entityType) return;
		this._props.entityType = value;
		const oldValue = this._filter;
		this._filter = (extension: ManifestEntityAction<MetaEntityAction>) => extension.meta.entityTypes.includes(value);
		this.requestUpdate('_filter', oldValue);
		this.requestUpdate('_props');
	}

	@state()
	_filter?: (extension: ManifestEntityAction<MetaEntityAction>) => boolean;

	@property({ type: String })
	public get unique(): string | null | undefined {
		return this._props.unique;
	}
	public set unique(value: string | null | undefined) {
		this._props.unique = value;
		this.requestUpdate('_props');
	}

	@state()
	_props: Partial<UmbEntityActionArgs<unknown>> = {};

	render() {
		return this._filter
			? html`
					<umb-extension-with-api-slot
						type="entityAction"
						default-element="umb-entity-action"
						.filter=${this._filter}
						.props=${this._props}
						.apiArgs=${[this._props]}></umb-extension-with-api-slot>
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
