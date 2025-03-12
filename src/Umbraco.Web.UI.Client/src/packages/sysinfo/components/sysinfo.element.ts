import { UmbSysinfoRepository } from '../repository/index.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbCurrentUserRepository } from '@umbraco-cms/backoffice/current-user';

type ServerKeyValue = {
	name: string;
	data: string;
};

@customElement('umb-sysinfo')
export class UmbSysinfoElement extends UmbModalBaseElement {
	@state()
	private _systemInformation = '';

	@state()
	private _loading = false;

	@state()
	private _buttonState?: UUIButtonState;

	readonly #serverKeyValues: Array<ServerKeyValue> = [];
	readonly #sysinfoRepository = new UmbSysinfoRepository(this);
	readonly #currentUserRepository = new UmbCurrentUserRepository(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#populate();
	}

	async #populate() {
		this._loading = true;
		this.#serverKeyValues.length = 0;

		const [serverTroubleshooting, serverInformation] = await Promise.all([
			this.#sysinfoRepository.requestTroubleShooting(),
			this.#sysinfoRepository.requestServerInformation(),
		]);

		if (serverTroubleshooting) {
			this.#serverKeyValues.push(...serverTroubleshooting.items);
		}

		if (serverInformation) {
			this.#serverKeyValues.push({ name: 'Umbraco build version', data: serverInformation.version });
			this.#serverKeyValues.push({ name: 'Server time offset', data: serverInformation.baseUtcOffset });
			this.#serverKeyValues.push({ name: 'Runtime mode', data: serverInformation.runtimeMode });
		}

		// Browser information
		this.#serverKeyValues.push({ name: 'Browser (user agent)', data: navigator.userAgent });
		this.#serverKeyValues.push({ name: 'Browser language', data: navigator.language });
		this.#serverKeyValues.push({ name: 'Browser location', data: location.href });

		// User information
		const { data: currentUser } = await this.#currentUserRepository.requestCurrentUser();
		if (currentUser) {
			this.#serverKeyValues.push({ name: 'User is admin', data: currentUser.isAdmin ? 'Yes' : 'No' });
			this.#serverKeyValues.push({ name: 'User sections', data: currentUser.allowedSections.join(', ') });
			this.#serverKeyValues.push({ name: 'User culture', data: currentUser.languageIsoCode });
			this.#serverKeyValues.push({
				name: 'User languages',
				data: currentUser.hasAccessToAllLanguages ? 'All' : currentUser.languages.join(', '),
			});
			let userDocumentAccess = 'None';
			if (currentUser.hasDocumentRootAccess) {
				userDocumentAccess = 'All';
			} else if (currentUser.documentStartNodeUniques.length) {
				userDocumentAccess = currentUser.documentStartNodeUniques.join(', ');
			}
			this.#serverKeyValues.push({
				name: 'User Document access',
				data: userDocumentAccess,
			});
			let userMediaAccess = 'None';
			if (currentUser.hasMediaRootAccess) {
				userMediaAccess = 'All';
			} else if (currentUser.mediaStartNodeUniques.length) {
				userMediaAccess = currentUser.mediaStartNodeUniques.join(', ');
			}
			this.#serverKeyValues.push({
				name: 'User Media access',
				data: userMediaAccess,
			});
		}

		this._systemInformation = this.#renderServerKeyValues();
		this._loading = false;
	}

	#renderServerKeyValues() {
		return this.#serverKeyValues
			.map((serverKeyValue) => {
				return `${serverKeyValue.name}: ${serverKeyValue.data}`;
			})
			.join('\n');
	}

	override render() {
		return html`
			<uui-dialog>
				<uui-dialog-layout headline="System information">
					${when(
						this._loading,
						() => html`<uui-loader-bar></uui-loader-bar>`,
						() => html` <umb-code-block id="codeblock">${this._systemInformation}</umb-code-block> `,
					)}

					<uui-button
						@click=${this._submitModal}
						slot="actions"
						look="secondary"
						label=${this.localize.term('general_close')}></uui-button>

					<uui-button
						@click=${this.#copyToClipboard}
						.state=${this._buttonState}
						slot="actions"
						look="primary"
						color="positive"
						label=${this.localize.term('clipboard_labelForCopyToClipboard')}></uui-button>
				</uui-dialog-layout>
			</uui-dialog>
		`;
	}

	async #copyToClipboard() {
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		try {
			this._buttonState = 'waiting';
			const text = `Umbraco system information
--------------------------------
${this._systemInformation}`;
			const textAsCode = `\`\`\`\n${text}\n\`\`\`\n`;
			await navigator.clipboard.writeText(textAsCode);

			setTimeout(() => {
				notificationContext?.peek('positive', {
					data: {
						headline: 'System information',
						message: this.localize.term('speechBubbles_copySuccessMessage'),
					},
				});
				this._buttonState = 'success';
			}, 250);
		} catch {
			this._buttonState = 'failed';
			notificationContext?.peek('danger', {
				data: {
					headline: 'System information',
					message: this.localize.term('speechBubbles_cannotCopyInformation'),
				},
			});
		}
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			#code-block {
				max-height: 300px;
			}
		`,
	];
}

export default UmbSysinfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-sysinfo': UmbSysinfoElement;
	}
}
