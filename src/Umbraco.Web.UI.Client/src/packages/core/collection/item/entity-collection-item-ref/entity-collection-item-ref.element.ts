import { UmbEntityCollectionItemElementBase } from '../umb-entity-collection-item-element-base.element.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

import './default-collection-item-ref.element.js';

@customElement('umb-entity-collection-item-ref')
export class UmbEntityCollectionItemRefElement extends UmbEntityCollectionItemElementBase {
	protected getExtensionType(): string {
		return 'entityCollectionItemRef';
	}

	protected createFallbackElement(): HTMLElement {
		return document.createElement('umb-default-collection-item-ref');
	}

	protected getPathAddendum(entityType: string, unique: string): string {
		return 'collection-item-ref/' + entityType + '/' + unique;
	}

	protected getMarkAttributeName(): string {
		return 'entity-collection-item-ref';
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
		'umb-entity-collection-item-ref': UmbEntityCollectionItemRefElement;
	}
}
