import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section/section.context';
import type { ManifestSection } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSections extends UmbLitElement {
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

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionStore) => {
			this._sectionContext = sectionStore;
			this._observeSections();
			this._observeCurrentSection();
		});
	}

	private _handleMore(e: MouseEvent) {
		e.stopPropagation();
		this._open = !this._open;
	}

	private _handleSectionTabClick(sectionManifest: ManifestSection) {
		this._sectionContext?.setManifest(sectionManifest);
	}

	private _handleLabelClick() {
		const moreTab = this.shadowRoot?.getElementById('moreTab');
		moreTab?.setAttribute('active', 'true');

		this._open = false;
	}

	private _observeSections() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.getAllowed(), (allowedSections) => {
			this._sections = allowedSections;
			this._visibleSections = this._sections;
		});
	}

	private _observeCurrentSection() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.alias, (currentSectionAlias) => {
			this._currentSectionAlias = currentSectionAlias || '';
		});
	}

	private _renderSections() {
		return html`
			<uui-tab-group id="tabs">
				${this._visibleSections.map(
					(section: ManifestSection) => html`
						<uui-tab
							@click="${() => this._handleSectionTabClick(section)}"
							?active="${this._currentSectionAlias === section.alias}"
							href="${`section/${section.meta.pathname}`}"
							label="${section.meta.label || section.name}"
							></uui-tab>
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
		'umb-backoffice-header-sections': UmbBackofficeHeaderSections;
	}
}
