import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionTextFilter } from '../filter/text/collection-text-filter.extension.js';
import type { UmbCollectionTextFilterElement } from '../filter/text/collection-text-filter-element.interface.js';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	@state()
	private _textFilter?: UmbCollectionTextFilterElement;

	@state()
	private _hasSlottedContent = false;

	constructor() {
		super();

		// Not `umb-extension-with-api-slot`: it reports nothing while manifests exist but none is permitted, and the filter
		// bundle has to be rendered in the toolbar in that case. With a text filter, the bundle goes in its `filters` slot.
		new UmbExtensionsElementAndApiInitializer<ManifestCollectionTextFilter, 'collectionTextFilter'>(
			this,
			umbExtensionsRegistry,
			'collectionTextFilter',
			undefined,
			undefined,
			(permitted) => {
				const textFilter = permitted[0]?.component;
				if (textFilter && !textFilter.querySelector('umb-collection-filter-bundle')) {
					// A new bundle rather than moving the toolbar's one: moving an element that has extension conditions in
					// one task can leave them unpermitted.
					textFilter.append(Object.assign(document.createElement('umb-collection-filter-bundle'), { slot: 'filters' }));
				}
				this._textFilter = textFilter;
			},
			undefined,
			undefined,
			undefined,
			{ single: true },
		);
	}

	#onSlotChange(event: Event) {
		this._hasSlottedContent = (event.target as HTMLSlotElement).assignedElements({ flatten: true }).length > 0;
	}

	override render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			${this._textFilter ?? nothing}
			<div id="slot" class=${this._textFilter ? 'shrink' : ''} ?hidden=${!this._hasSlottedContent}>
				<slot @slotchange=${this.#onSlotChange}></slot>
			</div>
			${this._textFilter || this._hasSlottedContent ? nothing : html`<div id="spacer"></div>`}
			${this._textFilter ? nothing : html`<umb-collection-filter-bundle></umb-collection-filter-bundle>`}
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

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

			/* With a text filter, the text filter grows and slotted content keeps its own width. */
			#slot.shrink {
				flex: 0 1 auto;
			}

			#slot[hidden] {
				display: none;
			}

			/* Pushes the items after it to the end when neither a text filter nor slotted content fills the space. */
			#spacer {
				flex: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
