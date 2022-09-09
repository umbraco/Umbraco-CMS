import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { Subscription } from 'rxjs';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../core/context';
import type { ManifestSection } from '../../core/models';
import { UmbSectionStore } from '../../core/stores/section.store';

@customElement('umb-backoffice-header-sections')
export class UmbBackofficeHeaderSections extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
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

	private _sectionStore?: UmbSectionStore;

	private _sectionSubscription?: Subscription;
	private _currentSectionSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbSectionStore', (sectionStore: UmbSectionStore) => {
			this._sectionStore = sectionStore;
			this._useSections();
			this._useCurrentSection();
		});
	}

	private _handleMore(e: MouseEvent) {
		e.stopPropagation();
		this._open = !this._open;
	}

	private _handleTabClick(e: PointerEvent) {
		const tab = e.currentTarget as HTMLElement;

		// TODO: we need to be able to prevent the tab from setting the active state
		if (tab.id === 'moreTab') return;

		if (!tab.dataset.alias) return;

		this._sectionStore?.setCurrent(tab.dataset.alias);
	}

	private _handleLabelClick() {
		const moreTab = this.shadowRoot?.getElementById('moreTab');
		moreTab?.setAttribute('active', 'true');

		this._open = false;
	}

	private _useSections() {
		this._sectionSubscription?.unsubscribe();

		this._sectionSubscription = this._sectionStore?.getAllowed().subscribe((allowedSections) => {
			this._sections = allowedSections;
			this._visibleSections = this._sections;
		});
	}

	private _useCurrentSection() {
		this._currentSectionSubscription?.unsubscribe();

		this._currentSectionSubscription = this._sectionStore?.currentAlias.subscribe((currentSectionAlias) => {
			this._currentSectionAlias = currentSectionAlias;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionSubscription?.unsubscribe();
	}

	private _renderSections() {
		return html`
			<uui-tab-group id="tabs">
				${this._visibleSections.map(
					(section: ManifestSection) => html`
						<uui-tab
							@click="${this._handleTabClick}"
							?active="${this._currentSectionAlias === section.alias}"
							href="${`/section/${section.meta.pathname}`}"
							label="${section.name}"
							data-alias="${section.alias}"></uui-tab>
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
				<uui-tab id="moreTab" @click="${this._handleTabClick}">
					<uui-popover .open=${this._open} placement="bottom-start" @close="${() => (this._open = false)}">
						<uui-button slot="trigger" look="primary" label="More" @click="${this._handleMore}" compact>
							<uui-symbol-more></uui-symbol-more>
						</uui-button>

						<div slot="popover" id="dropdown">
							${this._extraSections.map(
								(section) => html`
									<uui-menu-item
										?active="${this._currentSectionAlias === section.alias}"
										label="${section.name}"
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
