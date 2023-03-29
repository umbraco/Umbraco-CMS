import '../installer/shared/layout/installer-layout.element';
import './upgrader-view.element';

import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UpgradeResource, UpgradeSettingsResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-upgrader
 */
@customElement('umb-upgrader')
export class UmbUpgraderElement extends UmbLitElement {
	@state()
	private upgradeSettings?: UpgradeSettingsResponseModel;

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

		const { data, error } = await tryExecute(UpgradeResource.getUpgradeSettings());

		if (data) {
			this.upgradeSettings = data;
		} else if (error) {
			this.errorMessage = error.detail;
		}

		this.fetching = false;
	}

	_handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();
		this.errorMessage = '';
		this.upgrading = true;

		const { error } = await tryExecute(UpgradeResource.postUpgradeAuthorize());

		if (error) {
			this.errorMessage = error.detail || 'Unknown error, please try again';
		} else {
			history.pushState(null, '', '/');
		}

		this.upgrading = false;
	};
}

export default UmbUpgraderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-upgrader': UmbUpgraderElement;
	}
}
