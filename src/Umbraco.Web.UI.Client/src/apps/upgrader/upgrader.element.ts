import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UpgradeSettingsResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UpgradeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute, UmbApiError } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../installer/shared/layout/installer-layout.element.js';
import './upgrader-view.element.js';

/**
 * @element umb-upgrader
 */
@customElement('umb-upgrader')
export class UmbUpgraderElement extends UmbLitElement {
	@state()
	private _upgradeSettings?: UpgradeSettingsResponseModel;

	@state()
	private _fetching = true;

	@state()
	private _upgrading = false;

	@state()
	private _errorMessage = '';

	constructor() {
		super();
		this._setup();
	}

	override render() {
		return html`<umb-installer-layout data-test="upgrader">
			<umb-upgrader-view
				.fetching=${this._fetching}
				.upgrading=${this._upgrading}
				.settings=${this._upgradeSettings}
				.errorMessage=${this._errorMessage}
				@onAuthorizeUpgrade=${this.#handleSubmit}></umb-upgrader-view>
		</umb-installer-layout>`;
	}

	private async _setup() {
		this._fetching = true;

		const { data, error } = await tryExecute(this, UpgradeService.getUpgradeSettings(), { disableNotifications: true });

		if (data) {
			this._upgradeSettings = data;
		} else if (error) {
			this._errorMessage = UmbApiError.isUmbApiError(error)
				? (error.problemDetails.detail ?? 'Unknown error, please try again')
				: error.message;
		}

		this._fetching = false;
	}

	#handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();
		this._errorMessage = '';
		this._upgrading = true;

		const { error } = await tryExecute(this, UpgradeService.postUpgradeAuthorize());

		if (error) {
			this._errorMessage = UmbApiError.isUmbApiError(error)
				? (error.problemDetails.detail ?? 'Unknown error, please try again')
				: (error.message ?? 'Unknown error, please try again');
		} else {
			history.pushState(null, '', 'section/content');
		}

		this._upgrading = false;
	};
}

export default UmbUpgraderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-upgrader': UmbUpgraderElement;
	}
}
