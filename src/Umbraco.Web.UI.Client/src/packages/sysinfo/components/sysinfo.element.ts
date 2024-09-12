import { UmbSysinfoRepository } from '../repository/index.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

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

	#serverKeyValues: Array<ServerKeyValue> = [];
	#sysinfoRepository = new UmbSysinfoRepository(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});

		this.#populate();
	}

	async #populate() {
		this._loading = true;
		this.#serverKeyValues = [];

		const [serverTroubleshooting, serverInformation] = await Promise.all([
			this.#sysinfoRepository.requestTroubleShooting(),
			this.#sysinfoRepository.requestServerInformation(),
		]);

		if (serverTroubleshooting) {
			this.#serverKeyValues = serverTroubleshooting.items;
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
						() => html` <umb-code-block id="codeblock"> ${this._systemInformation} </umb-code-block> `,
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
		try {
			this._buttonState = 'waiting';
			const text = `Umbraco system information
--------------------------------
${this._systemInformation}`;
			const textAsCode = `\`\`\`\n${text}\n\`\`\`\n`;
			await navigator.clipboard.writeText(textAsCode);

			setTimeout(() => {
				this.#notificationContext?.peek('positive', {
					data: {
						headline: 'System information',
						message: this.localize.term('speechBubbles_copySuccessMessage'),
					},
				});
				this._buttonState = 'success';
			}, 250);
		} catch {
			this._buttonState = 'failed';
			this.#notificationContext?.peek('danger', {
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
