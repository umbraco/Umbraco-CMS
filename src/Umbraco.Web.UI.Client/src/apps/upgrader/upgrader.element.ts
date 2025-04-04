import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UpgradeSettingsResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UpgradeService, ApiError } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../installer/shared/layout/installer-layout.element.js';
import './upgrader-view.element.js';

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

	override render() {
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

		const { data, error } = await tryExecute(this, UpgradeService.getUpgradeSettings());

		if (data) {
			this.upgradeSettings = data;
		} else if (error) {
			this.errorMessage = error instanceof ApiError ? (error.body as any).detail : error.message;
		}

		this.fetching = false;
	}

	_handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();
		this.errorMessage = '';
		this.upgrading = true;

		const { error } = await tryExecute(this, UpgradeService.postUpgradeAuthorize());

		if (error) {
			this.errorMessage =
				error instanceof ApiError ? (error.body as any).detail : (error.message ?? 'Unknown error, please try again');
		} else {
			history.pushState(null, '', 'section/content');
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
