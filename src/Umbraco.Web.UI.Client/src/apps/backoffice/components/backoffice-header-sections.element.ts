import { UMB_BACKOFFICE_CONTEXT_TOKEN } from '../backoffice.context.js';
import type { UmbBackofficeContext } from '../backoffice.context.js';
import { css, CSSResultGroup, html, when, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbExtensionManifestInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSectionsElement extends UmbLitElement {
	@state()
	private _open = false;

	@state()
	private _sections: Array<UmbExtensionManifestInitializer<ManifestSection>> = [];

	@state()
	private _currentSectionAlias = '';

	private _backofficeContext?: UmbBackofficeContext;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT_TOKEN, (backofficeContext) => {
			this._backofficeContext = backofficeContext;
			this._observeSections();
			this._observeCurrentSection();
		});
	}

	private _handleMore(e: MouseEvent) {
		e.stopPropagation();
		this._open = !this._open;
	}

	private _handleLabelClick() {
		const moreTab = this.shadowRoot?.getElementById('moreTab');
		moreTab?.setAttribute('active', 'true');

		this._open = false;
	}

	private _observeSections() {
		if (!this._backofficeContext) return;

		this.observe(this._backofficeContext.allowedSections, (allowedSections) => {
			const oldValue = this._sections;
			this._sections = allowedSections;
			this.requestUpdate('_sections', oldValue);
		});
	}

	private _observeCurrentSection() {
		if (!this._backofficeContext) return;

		this.observe(this._backofficeContext.activeSectionAlias, (currentSectionAlias) => {
			this._currentSectionAlias = currentSectionAlias || '';
		});
	}

	private _renderSections() {
		return html`
			<uui-tab-group id="tabs">
				${repeat(
					this._sections,
					(section) => section.alias,
					(section) => html`
						<uui-tab
							?active="${this._currentSectionAlias === section.alias}"
							href="${`section/${section.manifest?.meta.pathname}`}"
							label="${section.manifest?.meta.label ?? section.manifest?.name ?? ''}"></uui-tab>
					`,
				)}
			</uui-tab-group>
		`;
	}

	render() {
		return html` ${this._renderSections()} `;
	}

	static styles: CSSResultGroup = [
		css`
			#tabs {
				color: var(--uui-color-header-contrast);
				height: 60px;
				font-size: 16px;
				--uui-tab-text: var(--uui-color-header-contrast);
				--uui-tab-text-hover: var(--uui-color-header-contrast-emphasis);
				--uui-tab-text-active: var(--uui-color-header-contrast-emphasis);
				--uui-tab-background: var(--uui-color-header-background);
			}

			#dropdown {
				background-color: white;
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
				min-width: 200px;
				color: black; /* Change to variable */
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-sections': UmbBackofficeHeaderSectionsElement;
	}
}
