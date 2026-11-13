import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionTextFilter } from '../filter/text/collection-text-filter.extension.js';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	/**
	 * One instance, so moving between the text filter and the toolbar keeps its state. It is placed in the text filter's
	 * `filters` slot, or rendered on its own when the collection has no text filter.
	 */
	#filterBundle: HTMLElement = Object.assign(document.createElement('umb-collection-filter-bundle'), {
		slot: 'filters',
	});

	@state()
	private _hasSlottedContent = false;

	#onSlotChange(event: Event) {
		this._hasSlottedContent = (event.target as HTMLSlotElement).assignedElements({ flatten: true }).length > 0;
	}

	override render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			<umb-extension-with-api-slot
				single
				type="collectionTextFilter"
				.renderMethod=${this.#renderTextFilter}
				.fallbackRenderMethod=${() => this.#filterBundle}></umb-extension-with-api-slot>
			<div id="slot" ?hidden=${!this._hasSlottedContent}><slot @slotchange=${this.#onSlotChange}></slot></div>
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

	#renderTextFilter = (extension: UmbExtensionElementAndApiInitializer<ManifestCollectionTextFilter>) => {
		extension.component?.append(this.#filterBundle);
		return extension.component;
	};

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				width: 100%;
			}
			#slot {
				flex: 1;
			}

			#slot[hidden] {
				display: none;
			}

			umb-collection-view-bundle {
				margin-left: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
