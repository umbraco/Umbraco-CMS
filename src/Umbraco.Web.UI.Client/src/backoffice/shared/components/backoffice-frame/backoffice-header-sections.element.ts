import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context';
import type { UmbBackofficeContext } from './backoffice.context';
import type { ManifestSection } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSectionsElement extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
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

	@state()
	private _open = false;

	@state()
	private _sections: Array<ManifestSection> = [];

	@state()
	private _visibleSections: Array<ManifestSection> = [];

	@state()
	private _extraSections: Array<ManifestSection> = [];

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

	private _handleSectionTabClick(alias: string) {
		this._backofficeContext?.setActiveSectionAlias(alias);
	}

	private _handleLabelClick() {
		const moreTab = this.shadowRoot?.getElementById('moreTab');
		moreTab?.setAttribute('active', 'true');

		this._open = false;
	}

	private _observeSections() {
		if (!this._backofficeContext) return;

		this.observe(this._backofficeContext.getAllowedSections(), (allowedSections) => {
			this._sections = allowedSections;
			this._visibleSections = this._sections;
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
				${this._visibleSections.map(
					(section: ManifestSection) => html`
						<uui-tab
							@click="${() => this._handleSectionTabClick(section.alias)}"
							?active="${this._currentSectionAlias === section.alias}"
							href="${`section/${section.meta.pathname}`}"
							label="${section.meta.label || section.name}"></uui-tab>
					`
				)}
				${this._renderExtraSections()}
			</uui-tab-group>
		`;
	}

	private _renderExtraSections() {
		return when(
			this._extraSections.length > 0,
			() => html`
				<uui-tab id="moreTab">
					<uui-popover .open=${this._open} placement="bottom-start" @close="${() => (this._open = false)}">
						<uui-button slot="trigger" look="primary" label="More" @click="${this._handleMore}" compact>
							<uui-symbol-more></uui-symbol-more>
						</uui-button>

						<div slot="popover" id="dropdown">
							${this._extraSections.map(
								(section) => html`
									<uui-menu-item
										?active="${this._currentSectionAlias === section.alias}"
										label="${section.meta.label || section.name}"
										@click-label="${this._handleLabelClick}"></uui-menu-item>
								`
							)}
						</div>
					</uui-popover>
				</uui-tab>
			`
		);
	}

	render() {
		return html` ${this._renderSections()} `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-sections': UmbBackofficeHeaderSectionsElement;
	}
}
