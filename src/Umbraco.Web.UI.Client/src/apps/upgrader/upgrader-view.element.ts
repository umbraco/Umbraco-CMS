import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, LitElement, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UpgradeSettingsResponseModelReadable } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * @element umb-upgrader-view
 * @fires {CustomEvent<SubmitEvent>} onAuthorizeUpgrade - fires when the user clicks the continue button
 */
@customElement('umb-upgrader-view')
export class UmbUpgraderViewElement extends LitElement {
	@property({ type: Boolean })
	fetching = false;

	@property({ type: Boolean })
	upgrading = false;

	@property({ type: String })
	errorMessage = '';

	@property({ type: Object, reflect: true })
	settings?: UpgradeSettingsResponseModelReadable;

	private _renderLayout() {
		return html`
				<h1>Upgrading Umbraco</h1>

				<p>
					Welcome to the Umbraco installer. You see this screen because your Umbraco installation needs a quick upgrade
					of its database and files, which will ensure your website is kept as fast, secure and up to date as possible.
				</p>

				<p>
					Detected current version <strong>${this.settings?.oldVersion}</strong> (${this.settings?.currentState}),
					which needs to be upgraded to <strong>${this.settings?.newVersion}</strong> (${this.settings?.newState}).
					To compare versions and read a report of changes between versions, use the View Report button below.
				</p>

				${
					this.settings?.reportUrl
						? html`
								<p>
									<uui-button
										data-test="view-report-button"
										look="secondary"
										href="${this.settings.reportUrl}"
										target="_blank"
										label="View Report"></uui-button>
								</p>
							`
						: ''
				}

				<p>Simply click <strong>continue</strong> below to be guided through the rest of the upgrade.</p>

				<form id="authorizeUpgradeForm" @submit=${this._handleSubmit}>
					<p>
						<uui-button
							data-test="continue-button"
							id="authorizeUpgrade"
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
		return html`
			${this.errorMessage ? html`<p class="error" data-test="error-message">${this.errorMessage}</p>` : ''}
		`;
	}

	override render() {
		return html` ${this.fetching ? html`<div class="center"><uui-loader></uui-loader></div>` : this._renderLayout()} `;
	}

	_handleSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		this.dispatchEvent(new CustomEvent('onAuthorizeUpgrade', { detail: e, bubbles: true }));
	};

	static override styles: CSSResultGroup = [
		css`
			.center {
				display: grid;
				place-items: center;
				height: 100vh;
			}
			.error {
				color: var(--uui-color-danger);
			}
		`,
	];
}

export default UmbUpgraderViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-upgrader-view': UmbUpgraderViewElement;
	}
}
