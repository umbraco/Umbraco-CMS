import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

@customElement('umb-entity-action-list')
class UmbEntityActionListElement extends UmbLitElement {
	private _entityType = '';
	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value) {
		const oldValue = this._entityType;
		this._entityType = value;
		if (oldValue !== this._entityType) {
			this.#observeEntityActions();
			this.requestUpdate('entityType', oldValue);
		}
	}

	@property({ type: String })
	public unique?: string;

	@state()
	private _entityActions?: Array<ManifestEntityAction>;

	// TODO: find a solution to use extension slot
	#observeEntityActions() {
		this.observe(
			umbExtensionsRegistry.extensionsOfType('entityAction').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.conditions.entityType === this.entityType);
				})
			),
			(actions) => {
				this._entityActions = actions;
			}
		);
	}

	render() {
		return html`
			${this._entityActions?.map(
				(manifest) => html`<umb-entity-action .unique=${this.unique} .manifest=${manifest}></umb-entity-action>`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action-list': UmbEntityActionListElement;
	}
}
