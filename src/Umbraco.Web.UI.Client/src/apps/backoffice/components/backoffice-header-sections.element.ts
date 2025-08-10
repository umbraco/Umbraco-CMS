import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import type { UmbBackofficeContext } from '../backoffice.context.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionManifestInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSectionsElement extends UmbLitElement {
	@state()
	private _sections: Array<UmbExtensionManifestInitializer<ManifestSection>> = [];

	@state()
	private _currentSectionAlias = '';

	private _backofficeContext?: UmbBackofficeContext;

	private _pathFromAlias = new Map<string, string>();
	private _aliasFromPath = new Map<string, string>();
	private _lastPathInSection = new Map<string, string>();

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (backofficeContext) => {
			this._backofficeContext = backofficeContext;
			this._observeSections();
			this._observeCurrentSection();
		});

		//window.addEventListener('willchangestate', this.#onWillNavigate);
	}

	private _observeSections() {
		if (!this._backofficeContext) return;

		this.observe(
			this._backofficeContext.allowedSections,
			(allowedSections) => {
				const oldValue = this._sections;
				this._sections = allowedSections;
				this._sections.forEach((section) => {
					if (section.manifest?.meta?.pathname) {
						this._pathFromAlias.set(section.alias, this.#getSectionRelativePath(section));
						this._aliasFromPath.set(this.#getSectionRelativePath(section), section.alias);
					}
				});
				this.requestUpdate('_sections', oldValue);
			},
			'observeSections',
		);
	}

	#prevPath?: string;
	#userClickedSection = false;

	#onWillNavigate = (e: CustomEvent) => {
		if (this.#userClickedSection === false) {
			return;
		}

		this.#prevPath = window.location.pathname;
		const nextPath = e.detail.url;

		console.log('Previous path:', this.#prevPath);
		console.log('Next path:', nextPath);

		// Check if the next path is a section root path and therefore a section change.
		const isSectionChange = this._aliasFromPath.has(nextPath);

		if (isSectionChange) {
			// Check if we have a previous path in the section.
			const newSectionAlias = this._aliasFromPath.get(nextPath);

			if (newSectionAlias && this._lastPathInSection.has(newSectionAlias)) {
				const lastPathInNewSection = this._lastPathInSection.get(newSectionAlias);
				if (lastPathInNewSection) {
					e.preventDefault();
					history.pushState(null, '', lastPathInNewSection);
					console.log('Navigating to last path in section:', lastPathInNewSection);
				}
			}
		}

		this.#userClickedSection = false;

		/*
		// Check if the next path is the root of the current section. This means that the section is being "reloaded".
		const isSectionReload = this._pathFromAlias.get(this._currentSectionAlias) === nextPath;

		/* If its a section reload, we let the navigation continue as normal. 
		 Navigating to the section root and doing a reload. */
		/*
		if (isSectionReload) {
			return;
		} else {
			const nextSectionAlias = this._aliasFromPath.get(nextPath);
			if (!nextSectionAlias) return;
			const lastPathInNewSection = this._lastPathInSection.get(nextSectionAlias);
			// If we have a last path in the section, we navigate to that instead.
			if (lastPathInNewSection) {
				e.preventDefault();
				history.pushState(null, '', lastPathInNewSection);
				debugger;
				return;
			} else {
				return;
			}
		}
		*/
	};

	private _observeCurrentSection() {
		if (!this._backofficeContext) return;

		this.observe(
			this._backofficeContext.activeSectionAlias,
			(newSectionAlias) => {
				// When the section changes, we store the last path that was navigated to in that section.
				// This is used to navigate back to the last path in the section when navigating to the section root.
				// Only store the last path if it is different from section root path.
				const currentSectionRootPath = this._pathFromAlias.get(this._currentSectionAlias || '');

				if (this._currentSectionAlias && this.#prevPath && this.#prevPath !== currentSectionRootPath) {
					// ensure that we only store a last path if it includes the section root path.
					if (this.#prevPath.startsWith(currentSectionRootPath || '')) {
						this._lastPathInSection.set(this._currentSectionAlias, this.#prevPath);
					}
				}

				this._currentSectionAlias = newSectionAlias || '';
			},
			'observeCurrentSection',
		);
	}

	#getSectionRelativePath(section: UmbExtensionManifestInitializer<ManifestSection>) {
		return `/section/${section.manifest?.meta.pathname}`;
	}

	#onSectionClick() {
		this.#userClickedSection = true;
	}

	override render() {
		return html`
			<uui-tab-group id="tabs" data-mark="section-links">
				${repeat(
					this._sections,
					(section) => section.alias,
					(section) => html`
						<uui-tab
							?active="${this._currentSectionAlias === section.alias}"
							href="${ifDefined(this.#getSectionRelativePath(section))}"
							label="${ifDefined(
								section.manifest?.meta.label
									? this.localize.string(section.manifest?.meta.label)
									: section.manifest?.name,
							)}"
							data-mark="section-link:${section.alias}"
							@click=${this.#onSectionClick}></uui-tab>
					`,
				)}
			</uui-tab-group>
		`;
	}

	static override styles: CSSResultGroup = [
		css`
			:host {
				display: contents;
			}
			#tabs {
				height: 60px;
				flex-basis: 100%;
				font-size: 16px; /* specific for the header */
				--uui-tab-text: var(--uui-color-header-contrast);
				--uui-tab-text-hover: var(--uui-color-header-contrast-emphasis);
				--uui-tab-text-active: var(--uui-color-header-contrast-emphasis);
				background-color: var(--uui-color-header-background);
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
