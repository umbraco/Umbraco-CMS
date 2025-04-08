import type { UmbInstallerContext } from '../installer.context.js';
import { UMB_INSTALLER_CONTEXT } from '../installer.context.js';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, nothing, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';

import type {
	DatabaseInstallRequestModel,
	DatabaseSettingsPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { InstallService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecute, UmbApiError, type UmbProblemDetails } from '@umbraco-cms/backoffice/resources';

@customElement('umb-installer-database')
export class UmbInstallerDatabaseElement extends UmbLitElement {
	@query('#button-install')
	private _installButton!: UUIButtonElement;

	@property({ attribute: false })
	public databaseFormData!: DatabaseInstallRequestModel;

	@state()
	private _options: Option[] = [];

	@state()
	private _databases: DatabaseSettingsPresentationModel[] = [];

	@state()
	private _preConfiguredDatabase?: DatabaseSettingsPresentationModel;

	@state()
	private _validationErrorMessage = '';

	private _installerContext?: UmbInstallerContext;

	constructor() {
		super();

		this.consumeContext(UMB_INSTALLER_CONTEXT, (installerContext) => {
			this._installerContext = installerContext;
			this._observeInstallerSettings();
			this._observeInstallerData();
		});
	}

	private _observeInstallerSettings() {
		if (!this._installerContext) return;

		this.observe(this._installerContext.settings, (settings) => {
			const databases = settings?.databases?.length ? [...settings.databases] : [];

			// Sort the databases array if not empty and by sortOrder if it exists
			databases.sort((a, b) => {
				if (a.sortOrder === undefined) {
					return -1;
				}
				if (b.sortOrder === undefined) {
					return 1;
				}
				return a.sortOrder - b.sortOrder;
			});

			this._databases = databases;

			// If there is an isConfigured database in the databases array then we can skip the database selection step
			// and just use that.
			this._preConfiguredDatabase = this._databases.find((x) => x.isConfigured);
			if (this._preConfiguredDatabase) {
				this._setDatabase({
					id: this._preConfiguredDatabase.id,
					providerName: this._preConfiguredDatabase.providerName,
					useIntegratedAuthentication: false,
					trustServerCertificate: false,
				});
			} else {
				this._options = this._databases
					.filter((x) => !!x.id)
					.map((x, i) => ({
						name: x.displayName ?? 'Unknown database',

						value: x.id!,
						selected: i === 0,
					}));
			}
		});
	}

	private _observeInstallerData() {
		if (!this._installerContext) return;

		this.observe(this._installerContext.data, (data) => {
			this.databaseFormData = data?.database ?? {};
			this._options.forEach((x, i) => (x.selected = data?.database?.id === x.value || i === 0));
		});
	}

	private _handleChange(e: InputEvent) {
		const target = e.target as HTMLInputElement;

		const value: { [key: string]: string | boolean } = {};
		value[target.name] = target.checked ?? target.value; // handle boolean and text inputs

		// TODO: Mark id and providerName as non-optional in schema
		const database = {
			id: '0',
			providerName: '',
			...this._installerContext?.getData().database,
			...value,
		} as DatabaseInstallRequestModel;

		this._setDatabase(database);
	}

	private _setDatabase(database: DatabaseInstallRequestModel) {
		this._installerContext?.appendData({ database });
	}

	private _handleSubmit = async (evt: SubmitEvent) => {
		evt.preventDefault();

		const form = evt.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		if (!this._installerContext) return;

		this._installButton.state = 'waiting';

		// Only append the database if it's not pre-configured
		if (!this._preConfiguredDatabase) {
			const formData = new FormData(form);
			const id = formData.get('id') as string;
			const username = formData.get('username') as string;
			const password = formData.get('password') as string;
			const server = formData.get('server') as string;
			const name = formData.get('name') as string;
			const useIntegratedAuthentication = formData.has('useIntegratedAuthentication');
			const trustServerCertificate = formData.has('trustServerCertificate');
			const connectionString = formData.get('connectionString') as string;

			// Validate connection
			const selectedDatabase = this._databases.find((x) => x.id === id);

			if (!selectedDatabase || !selectedDatabase.providerName || !selectedDatabase.id) {
				this._validationErrorMessage = 'No valid database selected';
				this._installButton.state = 'failed';
				return;
			}

			if (selectedDatabase.requiresConnectionTest) {
				const databaseDetails: DatabaseInstallRequestModel = {
					id,
					username,
					password,
					server,
					useIntegratedAuthentication,
					trustServerCertificate,
					name,
					connectionString,
					providerName: selectedDatabase.providerName,
				};

				const { error } = await tryExecute(
					this,
					InstallService.postInstallValidateDatabase({ requestBody: databaseDetails }),
					{ disableNotifications: true },
				);

				if (error) {
					this._validationErrorMessage = `The server could not validate the database connection. Details: ${
						UmbApiError.isUmbApiError(error) ? error.problemDetails.detail : error.message
					}`;
					this._installButton.state = 'failed';
					return;
				}
			}

			const database: DatabaseInstallRequestModel = {
				...this._installerContext?.getData().database,
				id,
				username,
				password,
				server,
				name,
				useIntegratedAuthentication,
				trustServerCertificate,
				connectionString,
				providerName: selectedDatabase.providerName,
			};

			this._installerContext.appendData({ database });
		}

		this._installerContext.nextStep();

		const { error } = await tryExecute(
			this,
			InstallService.postInstallSetup({ requestBody: this._installerContext.getData() }),
			{ disableNotifications: true },
		);
		if (error) {
			if (UmbApiError.isUmbApiError(error)) this._handleRejected(error.problemDetails);
		} else {
			this._handleFulfilled();
		}
	};

	private _handleFulfilled() {
		// TODO: The post install will probably return a user in the future, so we have to set that context somewhere to let the client know that it is authenticated
		console.warn('TODO: Set up real authentication');
		sessionStorage.setItem('is-authenticated', 'true');
		history.replaceState(null, '', 'section/content');
	}

	private _handleRejected(e: UmbProblemDetails) {
		this._installerContext?.setInstallStatus(e);
	}

	private _onBack() {
		this._installerContext?.prevStep();
	}

	private get _selectedDatabase() {
		const id = this._installerContext?.getData().database?.id;
		return this._databases.find((x) => x.id === id) ?? this._databases[0];
	}

	private _renderSettings() {
		if (!this._selectedDatabase) return;

		if (this._selectedDatabase.displayName?.toLowerCase() === 'custom') {
			return this._renderCustom();
		}

		const result = [];

		if (this._selectedDatabase.requiresServer) {
			result.push(this._renderServer());
		}

		result.push(
			this._renderDatabaseName(this.databaseFormData.name ?? this._selectedDatabase.defaultDatabaseName ?? 'umbraco'),
		);

		if (this._selectedDatabase.requiresCredentials) {
			result.push(this._renderCredentials());
		}

		return result;
	}

	private _renderServer = () => html`
		<h2 class="uui-h4">Connection</h2>
		<hr />
		<uui-form-layout-item>
			<uui-label for="server" slot="label" required>Server address</uui-label>
			<uui-input
				type="text"
				id="server"
				name="server"
				label="Server address"
				@input=${this._handleChange}
				.value=${this.databaseFormData.server ?? ''}
				.placeholder=${this._selectedDatabase?.serverPlaceholder ?? ''}
				required
				required-message="Server is required"></uui-input>
		</uui-form-layout-item>
	`;

	private _renderDatabaseName = (value: string) =>
		html` <uui-form-layout-item>
			<uui-label for="database-name" slot="label" required>Database Name</uui-label>
			<uui-input
				type="text"
				.value=${value}
				id="database-name"
				name="name"
				label="Database name"
				@input=${this._handleChange}
				placeholder="umbraco"
				required
				required-message="Database name is required"></uui-input>
		</uui-form-layout-item>`;

	private _renderCredentials = () => html`
		<h2 class="uui-h4">Credentials</h2>
		<hr />
		<uui-form-layout-item>
			<uui-checkbox
				name="useIntegratedAuthentication"
				label="Use integrated authentication"
				@change=${this._handleChange}
				.checked=${this.databaseFormData.useIntegratedAuthentication || false}></uui-checkbox>
		</uui-form-layout-item>
		<uui-form-layout-item>
			<uui-checkbox
				name="trustServerCertificate"
				label="Trust the database certificate"
				@change=${this._handleChange}
				.checked=${this.databaseFormData.trustServerCertificate || false}></uui-checkbox>
		</uui-form-layout-item>

			${
				!this.databaseFormData.useIntegratedAuthentication
					? html` <uui-form-layout-item>
								<uui-label for="username" slot="label" required>Username</uui-label>
								<uui-input
									type="text"
									.value=${this.databaseFormData.username ?? ''}
									id="username"
									name="username"
									label="Username"
									@input=${this._handleChange}
									required
									required-message="Username is required"></uui-input>
							</uui-form-layout-item>

							<uui-form-layout-item>
								<uui-label for="password" slot="label" required>Password</uui-label>
								<uui-input
									type="text"
									.value=${this.databaseFormData.password ?? ''}
									id="password"
									name="password"
									label="Password"
									@input=${this._handleChange}
									autocomplete="new-password"
									required
									required-message="Password is required"></uui-input>
							</uui-form-layout-item>`
					: ''
			}
		</uui-form-layout-item>
	`;

	private _renderCustom = () => html`
		<uui-form-layout-item>
			<uui-label for="connection-string" slot="label" required>Connection string</uui-label>
			<uui-textarea
				type="text"
				.value=${this.databaseFormData.connectionString ?? ''}
				id="connection-string"
				name="connectionString"
				label="connection-string"
				@input=${this._handleChange}
				required
				required-message="Connection string is required"></uui-textarea>
		</uui-form-layout-item>
	`;

	private _renderDatabaseSelection = () => html`
		<uui-form-layout-item>
			<uui-label for="database-type" slot="label" required>Database type</uui-label>
			<uui-select
				id="database-type"
				name="id"
				label="database-type"
				.options=${this._options || []}
				@change=${this._handleChange}></uui-select>
		</uui-form-layout-item>

		${this._renderSettings()}
	`;

	private _renderPreConfiguredDatabase = (database: DatabaseSettingsPresentationModel) => html`
		<p>A database has already been pre-configured on the server and will be used:</p>
		<p>
			Type: <strong>${database.displayName}</strong>
			<br />
			Provider: <strong>${database.providerName}</strong>
		</p>
	`;

	override render() {
		return html` <div id="container" class="uui-text" data-test="installer-database">
			<h1 class="uui-h3">Database Configuration</h1>
			<uui-form>
				<form id="database-form" name="database" @submit="${this._handleSubmit}">
					${this._preConfiguredDatabase
						? this._renderPreConfiguredDatabase(this._preConfiguredDatabase)
						: this._renderDatabaseSelection()}
					${this._validationErrorMessage ? html` <div class="error">${this._validationErrorMessage}</div> ` : nothing}

					<div id="buttons">
						<uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
						<uui-button id="button-install" type="submit" label="Install" look="primary" color="positive"></uui-button>
					</div>
				</form>
			</uui-form>
		</div>`;
	}

	static override styles: CSSResultGroup = [
		css`
			:host,
			#container {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			uui-form {
				height: 100%;
			}

			form {
				height: 100%;
				display: flex;
				flex-direction: column;
			}

			uui-form-layout-item {
				margin-top: 0;
				margin-bottom: var(--uui-size-layout-1);
			}

			uui-input,
			uui-input-password,
			uui-combobox {
				width: 100%;
			}

			hr {
				width: 100%;
				margin-top: var(--uui-size-space-2);
				margin-bottom: var(--uui-size-layout-1);
				border: none;
				border-bottom: 1px solid var(--uui-color-border);
			}

			h1 {
				text-align: center;
				margin-bottom: var(--uui-size-layout-3);
			}

			h2 {
				margin: 0;
			}

			#buttons {
				display: flex;
				margin-top: auto;
			}

			#button-install {
				margin-left: auto;
				min-width: 120px;
			}

			.error {
				color: var(--uui-color-danger);
				padding: var(--uui-size-space-2) 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-database': UmbInstallerDatabaseElement;
	}
}
