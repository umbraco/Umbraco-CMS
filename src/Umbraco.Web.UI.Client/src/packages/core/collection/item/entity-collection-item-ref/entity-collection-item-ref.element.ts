import { UmbEntityCollectionItemElementBase } from '../umb-entity-collection-item-element-base.element.js';
import { UmbDefaultCollectionItemRefElement } from './default-collection-item-ref.element.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-collection-item-ref')
export class UmbEntityCollectionItemRefElement extends UmbEntityCollectionItemElementBase {
	protected getExtensionType(): string {
		return 'entityCollectionItemRef';
	}

	protected createFallbackElement(): HTMLElement {
		return new UmbDefaultCollectionItemRefElement();
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
