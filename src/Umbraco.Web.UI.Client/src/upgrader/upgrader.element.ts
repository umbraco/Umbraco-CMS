import '../installer/installer-layout.element';
import './upgrader-view.element';

import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getUpgradeSettings, PostUpgradeAuthorize } from '../core/api/fetcher';

import type { UmbracoUpgrader } from '../core/models';

/**
 * @element umb-upgrader
 */
@customElement('umb-upgrader')
export class UmbUpgrader extends LitElement {
	@state()
	private upgradeSettings?: UmbracoUpgrader;

	@state()
	private fetching = true;

	@state()
	private upgrading = false;

	@state()
	private errorMessage = '';

	constructor() {
		super();
		this._setup();
	}

	render() {
		return html`<umb-installer-layout data-test="upgrader">
			<umb-upgrader-view
				.fetching=${this.fetching}
				.upgrading=${this.upgrading}
				.settings=${this.upgradeSettings}
				.errorMessage=${this.errorMessage}
				@onAuthorizeUpgrade=${this._handleSubmit}></umb-upgrader-view>
		</umb-installer-layout>`;
	}

	private async _setup() {
		this.fetching = true;

		try {
			const { data } = await getUpgradeSettings({});

			this.upgradeSettings = data;
		} catch (e) {
			if (e instanceof getUpgradeSettings.Error) {
				this.errorMessage = e.data.detail;
			}
		}

		this.fetching = false;
	}

	_handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();
		this.errorMessage = '';
		this.upgrading = true;

		try {
			await PostUpgradeAuthorize({});
			history.pushState(null, '', '/');
		} catch (e) {
			if (e instanceof PostUpgradeAuthorize.Error) {
				const error = e.getActualType();
				if (error.status === 400) {
					this.errorMessage = error.data.detail || 'Unknown error, please try again';
				}
			} else {
				this.errorMessage = 'Unknown error, please try again';
			}
		}

		this.upgrading = false;
	};
}

export default UmbUpgrader;

declare global {
	interface HTMLElementTagNameMap {
		'umb-upgrader': UmbUpgrader;
	}
}
