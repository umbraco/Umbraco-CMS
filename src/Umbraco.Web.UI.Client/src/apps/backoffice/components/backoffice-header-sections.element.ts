import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import type { UmbBackofficeContext } from '../backoffice.context.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSectionsElement extends UmbLitElement {
	@state()
	private _sections: Array<ManifestSection> = [];

	@state()
	private _currentSectionAlias = '';

	private _backofficeContext?: UmbBackofficeContext;

	#sectionPathMap = new Map<string, string>();

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (backofficeContext) => {
			this._backofficeContext = backofficeContext;
			this._observeSections();
			this._observeCurrentSection();
		});
	}

	private _observeSections() {
		if (!this._backofficeContext) return;

		this.observe(
			this._backofficeContext.allowedSections,
			(allowedSections) => {
				const oldValue = this._sections;
				this._sections = allowedSections.map((section) => section.manifest).filter((section) => section !== undefined);
				this.requestUpdate('_sections', oldValue);
			},
			'observeSections',
		);
	}

	private _observeCurrentSection() {
		if (!this._backofficeContext) return;

		this.observe(
			this._backofficeContext.activeSectionAlias,
			(currentSectionAlias) => {
				this._currentSectionAlias = currentSectionAlias || '';
			},
			'observeCurrentSection',
		);
	}

	#getSectionPath(manifest: ManifestSection | undefined) {
		return `section/${manifest?.meta.pathname}`;
	}

	#onSectionClick(event: PointerEvent, manifest: ManifestSection | undefined) {
		// Let the browser handle the click if the Ctrl or Meta key is pressed
		if (event.ctrlKey || event.metaKey) {
			return;
		}

		event.stopPropagation();
		event.preventDefault();

		// Store the current path for the section so we can redirect to it next time the section is visited
		if (this._currentSectionAlias) {
			const currentPath = window.location.pathname;
			this.#sectionPathMap.set(this._currentSectionAlias, currentPath);
		}

		if (!manifest) {
			throw new Error('Section manifest is missing');
		}

		const clickedSectionAlias = manifest.alias;

		// If preventUrlRetention is set to true then go to the section root.
		// Or if the clicked section is the current active one, then navigate to the section root
		if (manifest?.meta.preventUrlRetention === true || this._currentSectionAlias === clickedSectionAlias) {
			const sectionPath = this.#getSectionPath(manifest);
			history.pushState(null, '', sectionPath);
			return;
		}

		// Check if we have a stored path for the clicked section
		if (this.#sectionPathMap.has(clickedSectionAlias)) {
			const storedPath = this.#sectionPathMap.get(clickedSectionAlias);
			history.pushState(null, '', storedPath);
		} else {
			// Nothing stored, so we navigate to the regular section path
			const sectionPath = this.#getSectionPath(manifest);
			history.pushState(null, '', sectionPath);
		}
	}

	override render() {
		return html`
			<uui-tab-group id="tabs" data-mark="section-links">
				${repeat(
					this._sections,
					(section) => section.alias,
					(section) => this.#renderItem(section),
				)}
			</uui-tab-group>
		`;
	}

	#renderItem(manifest: ManifestSection) {
		const label = this.localize.string(manifest?.meta.label || manifest?.name);
		return html`<uui-tab
			data-mark="section-link:${manifest.alias}"
			.href=${this.#getSectionPath(manifest)}
			label=${ifDefined(label)}
			?active=${this._currentSectionAlias === manifest.alias}
			@click=${(event: PointerEvent) => this.#onSectionClick(event, manifest)}></uui-tab>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: contents;
			}
			#tabs {
				height: 60px;
				flex-basis: 100%;
				font-size: 16px; /* specific for the header */
				background-color: var(--uui-color-header-background);
				--uui-tab-text: var(--uui-color-header-contrast);
				--uui-tab-text-hover: var(--uui-color-header-contrast-emphasis);
				--uui-tab-text-active: var(--uui-color-header-contrast-emphasis);
				--uui-tab-group-dropdown-background: var(--uui-color-header-surface);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-sections': UmbBackofficeHeaderSectionsElement;
	}
}
