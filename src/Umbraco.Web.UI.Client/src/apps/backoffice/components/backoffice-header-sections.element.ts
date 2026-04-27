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

	readonly #mobileQuery = window.matchMedia('(max-width: 920px)');
	#sectionPathMap = new Map<string, string>();
	#tabGroup?: Element;

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

	protected override firstUpdated(): void {
		// Overflow proxy tabs are cloneNode copies without Lit event handlers; delegate to the host
		// and identify the clicked tab via data-mark. #onSectionClick stops propagation for visible tabs.
		this.#tabGroup = this.renderRoot.querySelector('#tabs') ?? undefined;
		this.#tabGroup?.addEventListener('click', this.#onTabGroupClick);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#tabGroup?.removeEventListener('click', this.#onTabGroupClick);
	}

	#onTabGroupClick = (e: Event) => {
		if (!this.#mobileQuery.matches) return;
		const path = e.composedPath() as HTMLElement[];
		const tab = path.find((el) => el.getAttribute?.('data-mark')?.startsWith('section-link:')) as HTMLElement | null;
		if (!tab) return;
		const alias = tab.getAttribute('data-mark')?.replace('section-link:', '');
		if (!alias) return;
		const section = this._sections.find((s) => s.alias === alias);
		if (!section) return;
		e.preventDefault();
		e.stopPropagation();
		history.pushState(null, '', this.#getSectionPath(section));
	};

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

		// If preventUrlRetention is set, or the clicked section is already active, always go to the section root.
		const targetPath =
			manifest.meta.preventUrlRetention === true || this._currentSectionAlias === clickedSectionAlias
				? this.#getSectionPath(manifest)
				: (this.#sectionPathMap.get(clickedSectionAlias) ?? this.#getSectionPath(manifest));

		history.pushState(null, '', targetPath);
	}

	#onCurrentSectionClick(event: PointerEvent) {
		if (event.ctrlKey || event.metaKey) return;
		event.preventDefault();
		this._backofficeContext?.toggleMobileSidebar();
	}

	override render() {
		const activeSection = this._sections.find((s) => s.alias === this._currentSectionAlias);
		const activeLabel = activeSection ? this.localize.string(activeSection.meta.label || activeSection.name) : undefined;

		return html`
			<uui-tab
				id="current-section-tab"
				data-mark="current-section"
				label=${ifDefined(activeLabel)}
				aria-label=${ifDefined(activeLabel ? `${activeLabel} — toggle navigation` : undefined)}
				@click=${this.#onCurrentSectionClick}></uui-tab>
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
			#current-section-tab {
				display: none;
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
			@media (max-width: 920px) {
				#current-section-tab {
					display: flex;
					height: 60px;
					font-size: 16px;
					--uui-tab-text: var(--uui-color-header-contrast);
					--uui-tab-text-hover: var(--uui-color-header-contrast-emphasis);
					--uui-tab-text-active: var(--uui-color-header-contrast);
					--uui-tab-divider: transparent;
				}
				#tabs {
					flex-basis: auto;
					max-width: 32px;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-sections': UmbBackofficeHeaderSectionsElement;
	}
}
