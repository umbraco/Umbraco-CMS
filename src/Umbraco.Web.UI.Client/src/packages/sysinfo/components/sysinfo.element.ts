import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbSysinfoRepository } from '../repository/sysinfo.repository';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

type ServerKeyValue = {
	name: string;
	data: string;
};

@customElement('umb-sysinfo')
export class UmbSysinfoElement extends UmbModalBaseElement {
	@state()
	private _serverKeyValues: Array<ServerKeyValue> = [];

	@state()
	private _loading = false;

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
		this._serverKeyValues = [];

		const [serverTroubleshooting, serverInformation] = await Promise.all([
			this.#sysinfoRepository.requestTroubleShooting(),
			this.#sysinfoRepository.requestServerInformation(),
		]);

		if (serverTroubleshooting) {
			this._serverKeyValues = [...this._serverKeyValues, ...serverTroubleshooting.items];
		}

		if (serverInformation) {
			this._serverKeyValues.push({ name: 'Umbraco build version', data: serverInformation.version });
			this._serverKeyValues.push({ name: 'Server time offset', data: serverInformation.baseUtcOffset });
			this._serverKeyValues.push({ name: 'Runtime mode', data: serverInformation.runtimeMode });
		}

		// Browser information
		this._serverKeyValues.push({ name: 'Browser (user agent)', data: navigator.userAgent });
		this._serverKeyValues.push({ name: 'Browser language', data: navigator.language });
		this._serverKeyValues.push({ name: 'Browser location', data: location.href });

		this._loading = false;
	}

	#renderServerKeyValues() {
		return this._serverKeyValues.map((serverKeyValue) => {
			return html`
				<uui-table-row>
					<uui-table-cell>${serverKeyValue.name}</uui-table-cell>
					<uui-table-cell>${serverKeyValue.data}</uui-table-cell>
				</uui-table-row>
			`;
		});
	}

	override render() {
		return html`
			<uui-dialog>
				<uui-dialog-layout headline="System information">
					${when(
						this._loading,
						() => html`<uui-loader-bar></uui-loader-bar>`,
						() => html`
							<uui-box id="sysinfo-table">
								<uui-table aria-label="System information">
									<uui-table-column style="width: 20%"></uui-table-column>
									<uui-table-column style="width: 80%"></uui-table-column>

									<uui-table-head>
										<uui-table-head-cell>Name</uui-table-head-cell>
										<uui-table-head-cell>Data</uui-table-head-cell>
									</uui-table-head>

									${this.#renderServerKeyValues()}
								</uui-table>
							</uui-box>
						`,
					)}

					<uui-button
						@click=${this.#copyToClipboard}
						slot="actions"
						look="secondary"
						label=${this.localize.term('clipboard_labelForCopyToClipboard')}></uui-button>

					<uui-button
						@click=${this._submitModal}
						slot="actions"
						look="secondary"
						label=${this.localize.term('general_close')}></uui-button>
				</uui-dialog-layout>
			</uui-dialog>
		`;
	}

	#copyToClipboard() {
		try {
			const serverKeyValues = this._serverKeyValues
				.map((serverKeyValue) => `${serverKeyValue.name}: ${serverKeyValue.data}`)
				.join('\n');
			const text = `
Umbraco system information\n
--------------------------------\n
${serverKeyValues}`;
			navigator.clipboard.writeText(text);

			this.#notificationContext?.peek('positive', {
				data: {
					headline: 'System information',
					message: this.localize.term('speechBubbles_copySuccessMessage'),
				},
			});
		} catch {
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
			#sysinfo-table {
				--uui-box-default-padding: 0;
				width: 100%;
				max-height: 300px;
				overflow: auto;
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
