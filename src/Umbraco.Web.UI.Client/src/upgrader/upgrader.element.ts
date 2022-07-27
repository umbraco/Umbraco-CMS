import '../installer/installer-layout.element';

import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';

import { getUpgradeSettings, PostUpgradeAuthorize } from '../core/api/fetcher';
import { UmbContextProviderMixin } from '../core/context';
import { UmbracoUpgrader } from '../core/models';

@customElement('umb-upgrader')
export class UmbUpgrader extends UmbContextProviderMixin(LitElement) {
	static styles: CSSResultGroup = [
		css`
			.error {
				color: var(--uui-color-danger);
			}
		`,
	];

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

	private _renderLayout() {
		return html`
				<h1>Upgrading Umbraco</h1>

				<p>
					Welcome to the Umbraco installer. You see this screen because your Umbraco installation needs a quick upgrade
					of its database and files, which will ensure your website is kept as fast, secure and up to date as possible.
				</p>

				<p>
					Detected current version <strong>${this.upgradeSettings?.oldVersion}</strong> (${this.upgradeSettings?.currentState}),
					which needs to be upgraded to <strong>${this.upgradeSettings?.newVersion}</strong> (${this.upgradeSettings?.newState}).
					To compare versions and read a report of changes between versions, use the View Report button below.
				</p>

				<p>
					<uui-button
						look="secondary"
						href="${ifDefined(this.upgradeSettings?.reportUrl)}"
						target="_blank"
						label="View Report"></uui-button>
				</p>

				<p>Simply click <strong>continue</strong> below to be guided through the rest of the upgrade.</p>

				<form @submit=${this._continue}>
					<p>
						<uui-button
							type="submit"
							look="primary"
							color="positive"
							label="Continue"
							state=${ifDefined(this.upgrading ? 'waiting' : undefined)}></uui-button>
					</p>
				</form>

				${this._renderError()}
			</div>
		`;
	}

	private _renderError() {
		return html` ${this.errorMessage ? html`<p class="error">${this.errorMessage}</p>` : ''} `;
	}

	render() {
		return html` <umb-installer-layout>
			<div id="container" class="uui-text">${this.fetching ? html`<div>Loading...</div>` : this._renderLayout()}</div>
		</umb-installer-layout>`;
	}

	private async _setup() {
		this.fetching = true;

		try {
			const { data } = await getUpgradeSettings({});

			this.upgradeSettings = data;
		} catch (e) {
			if (e instanceof getUpgradeSettings.Error) {
				this.errorMessage = e.message;
			}
		}

		this.fetching = false;
	}

	private _continue = async (e: SubmitEvent) => {
		e.preventDefault();
		this.errorMessage = '';
		this.upgrading = true;

		try {
			await PostUpgradeAuthorize({});

			history.pushState(null, '', '/');
		} catch (e) {
			if (e instanceof PostUpgradeAuthorize.Error) {
				const error = e.getActualType();
				if (error.status === 400) {
					this.errorMessage = error.data.detail || 'Unknown error';
				}
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
