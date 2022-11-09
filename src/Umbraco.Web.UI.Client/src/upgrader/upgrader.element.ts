import '../installer/shared/layout/installer-layout.element';
import './upgrader-view.element';

import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ApiError, ProblemDetails, UpgradeResource, UpgradeSettings } from '@umbraco-cms/backend-api';

/**
 * @element umb-upgrader
 */
@customElement('umb-upgrader')
export class UmbUpgrader extends LitElement {
	@state()
	private upgradeSettings?: UpgradeSettings;

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
			const data = await UpgradeResource.getUmbracoManagementApiV1UpgradeSettings();
			this.upgradeSettings = data;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e.body as ProblemDetails;
				this.errorMessage = error.detail;
			}
		}

		this.fetching = false;
	}

	_handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();
		this.errorMessage = '';
		this.upgrading = true;

		try {
			await UpgradeResource.postUmbracoManagementApiV1UpgradeAuthorize();
			history.pushState(null, '', '/');
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e.body as ProblemDetails;
				if (e.status === 400) {
					this.errorMessage = error.detail || 'Unknown error, please try again';
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
