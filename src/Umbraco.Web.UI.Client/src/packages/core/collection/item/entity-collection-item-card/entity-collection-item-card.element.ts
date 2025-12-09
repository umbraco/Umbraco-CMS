import { UmbEntityCollectionItemElementBase } from '../umb-entity-collection-item-element-base.element.js';
import { UmbDefaultCollectionItemCardElement } from './default-collection-item-card.element.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-collection-item-card')
export class UmbEntityCollectionItemCardElement extends UmbEntityCollectionItemElementBase {
	protected getExtensionType(): string {
		return 'entityCollectionItemCard';
	}

	protected createFallbackElement(): HTMLElement {
		return new UmbDefaultCollectionItemCardElement();
	}

	protected getPathAddendum(entityType: string, unique: string): string {
		return 'collection-item-card/' + entityType + '/' + unique;
	}

	protected getMarkAttributeName(): string {
		return 'entity-collection-item-card';
	}

	override render() {
		return html`${this._component}`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				position: relative;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-collection-item-card': UmbEntityCollectionItemCardElement;
	}
}
