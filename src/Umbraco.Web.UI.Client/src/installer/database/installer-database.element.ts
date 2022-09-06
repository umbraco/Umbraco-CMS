import { UUIButtonElement } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';

import { postInstallSetup, postInstallValidateDatabase } from '../../core/api/fetcher';
import { UmbContextConsumerMixin } from '../../core/context';
import type { UmbracoInstallerDatabaseModel, UmbracoPerformInstallDatabaseConfiguration } from '../../core/models';
import { UmbInstallerContext } from '../installer.context';

@customElement('umb-installer-database')
export class UmbInstallerDatabaseElement extends UmbContextConsumerMixin(LitElement) {
	static styles: CSSResultGroup = [
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
				margin-bottom: var(--uui-size-space-6);
			}

			uui-input,
			uui-input-password,
			uui-combobox {
				width: 100%;
			}

			hr {
				width: 100%;
				margin-top: var(--uui-size-space-2);
				margin-bottom: var(--uui-size-space-6);
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

	@query('#button-install')
	private _installButton!: UUIButtonElement;

	@property({ attribute: false })
	public databaseFormData!: UmbracoPerformInstallDatabaseConfiguration;

	@state()
	private _options: { name: string; value: string; selected?: boolean }[] = [];

	@state()
	private _databases: UmbracoInstallerDatabaseModel[] = [];

	@state()
	private _preConfiguredDatabase?: UmbracoInstallerDatabaseModel;

	@state()
	private _validationErrorMessage = '';

	private _installerContext?: UmbInstallerContext;
	private _installerDataSubscription?: Subscription;
	private _installerSettingsSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbInstallerContext', (installerContext: UmbInstallerContext) => {
			this._installerContext = installerContext;
			this._observeInstallerSettings();
			this._observeInstallerData();
		});
	}

	private _observeInstallerSettings() {
		this._installerSettingsSubscription?.unsubscribe();
		this._installerSettingsSubscription = this._installerContext?.settings.subscribe((settings) => {
			this._databases = settings.databases;

			// If there is an isConfigured database in the databases array then we can skip the database selection step
			// and just use that.
			this._preConfiguredDatabase = this._databases.find((x) => x.isConfigured);
			if (!this._preConfiguredDatabase) {
				this._options = settings.databases.map((x, i) => ({ name: x.displayName, value: x.id, selected: i === 0 }));
			}
		});
	}

	private _observeInstallerData() {
		this._installerDataSubscription?.unsubscribe();
		this._installerDataSubscription = this._installerContext?.data.subscribe((data) => {
			this.databaseFormData = data.database ?? {};
			this._options.forEach((x, i) => (x.selected = data.database?.id === x.value || i === 0));
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._installerSettingsSubscription?.unsubscribe();
		this._installerDataSubscription?.unsubscribe();
	}

	private _handleChange(e: InputEvent) {
		const target = e.target as HTMLInputElement;

		const value: { [key: string]: string | boolean } = {};
		value[target.name] = target.checked ?? target.value; // handle boolean and text inputs

		const database = { ...this._installerContext?.getData().database, ...value };

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
			const connectionString = formData.get('connectionString') as string;

			// Validate connection
			const selectedDatabase = this._databases.find((x) => x.id === id);
			if (selectedDatabase?.requiresConnectionTest) {
				try {
					let databaseDetails: UmbracoPerformInstallDatabaseConfiguration = {};

					if (connectionString) {
						databaseDetails.connectionString = connectionString;
					} else {
						databaseDetails = {
							id,
							username,
							password,
							server,
							useIntegratedAuthentication,
							name,
						};
					}
					await postInstallValidateDatabase(databaseDetails);
				} catch (e) {
					if (e instanceof postInstallSetup.Error) {
						const error = e.getActualType();
						console.warn('Database validation failed', error.data);
						this._validationErrorMessage = error.data.detail ?? 'Could not verify database connection';
					} else {
						this._validationErrorMessage = 'A server error happened when trying to validate the database';
					}
					this._installButton.state = 'failed';
					return;
				}
			}

			const database = {
				...this._installerContext?.getData().database,
				id,
				username,
				password,
				server,
				name,
				useIntegratedAuthentication,
				connectionString,
			} as UmbracoPerformInstallDatabaseConfiguration;

			this._installerContext?.appendData({ database });
		}

		this._installerContext?.nextStep();
		this._installerContext
			.requestInstall()
			.then(() => this._handleFulfilled())
			.catch((error) => this._handleRejected(error));
	};

	private _handleFulfilled() {
		console.warn('TODO: Set up real authentication');
		sessionStorage.setItem('is-authenticated', 'true');
		history.replaceState(null, '', '/content');
	}

	private _handleRejected(e: unknown) {
		if (e instanceof postInstallSetup.Error) {
			const error = e.getActualType();
			if (error.status === 400) {
				this._installerContext?.setInstallStatus(error.data);
			}
		}
		this._installerContext?.nextStep();
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

		if (this._selectedDatabase.displayName.toLowerCase() === 'custom') {
			return this._renderCustom();
		}

		const result = [];

		if (this._selectedDatabase.requiresServer) {
			result.push(this._renderServer());
		}

		result.push(this._renderDatabaseName(this.databaseFormData.name ?? this._selectedDatabase.defaultDatabaseName));

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

	private _renderDatabaseName = (value: string) => html` <uui-form-layout-item>
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
				.checked=${this.databaseFormData.useIntegratedAuthentication || false}>
				Use integrated authentication
			</uui-checkbox>
		</uui-form-layout-item>

		${!this.databaseFormData.useIntegratedAuthentication
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
			: ''}
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

	private _renderPreConfiguredDatabase = (database: UmbracoInstallerDatabaseModel) => html`
		<p>A database has already been pre-configured on the server and will be used:</p>
		<p>
			Type: <strong>${database.displayName}</strong>
			<br />
			Provider: <strong>${database.providerName}</strong>
		</p>
	`;

	render() {
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-database': UmbInstallerDatabaseElement;
	}
}
