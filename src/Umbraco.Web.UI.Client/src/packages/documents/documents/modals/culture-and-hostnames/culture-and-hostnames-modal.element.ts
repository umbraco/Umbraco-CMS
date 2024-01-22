import { UmbDocumentRepository } from '../../repository/document.repository.js';
import { html, customElement, state, css, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLanguageRepository } from '@umbraco-cms/backoffice/language';
import { DomainPresentationModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbCultureAndHostnamesModalData, UmbCultureAndHostnamesModalValue } from '@umbraco-cms/backoffice/document';

@customElement('umb-culture-and-hostnames-modal')
export class UmbCultureAndHostnamesModalElement extends UmbModalBaseElement<
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue
> {
	#documentRepository = new UmbDocumentRepository(this);
	#languageRepository = new UmbLanguageRepository(this);

	#unique?: string | null;
	#languageModel: Array<LanguageResponseModel> = [];

	@state()
	private _options: Array<Option> = [];

	@state()
	private _domains: Array<DomainPresentationModel> = [];

	#handleCancel() {
		this.modalContext?.reject();
	}
	#handleSave() {
		// TODO validation before submitting
		this.#documentRepository.saveCultureAndHostnames(this.#unique!, this.value);
		this.modalContext?.submit();
	}

	firstUpdated() {
		this.#unique = this.data?.unique;
		this.#getDomains();
	}

	async #getDomains() {
		if (!this.#unique) return;
		const { data } = await this.#documentRepository.getCultureAndHostnames(this.#unique);

		if (!data) return;
		this._domains = data.domains.map((domain) => ({ isoCode: domain.isoCode, domainName: domain.domainName }));

		this.value = { defaultIsoCode: data.defaultIsoCode, domains: this._domains };
		this.#getLanguages(data.defaultIsoCode);
	}

	async #getLanguages(defaultIsoCode?: string | null) {
		const { data } = await this.#languageRepository.requestLanguages();
		if (!data) return;

		this.#languageModel = data.items;

		const options = data.items.map((item) => ({
			name: item.name,
			selected: item.isoCode === defaultIsoCode,
			value: item.isoCode,
		}));
		options.unshift({ value: 'inherit', name: 'Inherit', selected: defaultIsoCode ? false : true });
		this._options = options;
	}

	#onChangeLanguage(e: UUISelectEvent) {
		const documentIsoCode = e.target.value as string;
		if (documentIsoCode === 'inherit') {
			this.value = { ...this.value, defaultIsoCode: undefined };
		} else {
			this.value = { ...this.value, defaultIsoCode: e.target.value as string };
		}
	}

	#addDomain(currentDomain?: boolean) {
		const defaultModel = this.#languageModel.find((model) => model.isDefault);
		if (currentDomain) {
			this._domains = [...this._domains, { isoCode: defaultModel?.isoCode ?? '', domainName: window.location.host }];
		} else {
			this._domains = [...this._domains, { isoCode: defaultModel?.isoCode ?? '', domainName: '' }];
		}

		this.value = { ...this.value, domains: this._domains };
	}

	#remove(index: number) {
		this._domains = this._domains.filter((d, i) => index !== i);

		this.value = { ...this.value, domains: this._domains };
	}

	#changeDomainName(e: UUIInputEvent, index: number) {
		const domainName = e.target.value as string;
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, domainName: domainName } : domain));

		this.value = { ...this.value, domains: this._domains };
	}

	#changeIsoCode(e: UUISelectEvent, index: number) {
		const isoCode = e.target.value as string;
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, isoCode: isoCode } : domain));

		this.value = { ...this.value, domains: this._domains };
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_assigndomain')}>
				<uui-box headline=${this.localize.term('assignDomain_setLanguage')}>
					<uui-label for="select">${this.localize.term('assignDomain_language')}</uui-label>
					<uui-select
						id="select"
						label=${this.localize.term('assignDomain_language')}
						@change=${this.#onChangeLanguage}
						.options=${this._options}></uui-select>
				</uui-box>
				<uui-box headline=${this.localize.term('assignDomain_setDomains')}>
					<umb-localize key="assignDomain_domainHelpWithVariants">
						Valid domain names are: "example.com", "www.example.com", "example.com:8080", or
						"https://www.example.com/".<br />Furthermore also one-level paths in domains are supported, eg.
						"example.com/en" or "/en".
					</umb-localize>
					${this.#renderDomains()} ${this.#renderAddNewDomainButton()}
				</uui-box>
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this.#handleCancel}"></uui-button>
				<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="positive"
					label=${this.localize.term('buttons_save')}
					@click="${this.#handleSave}"></uui-button>
			</umb-body-layout>
		`;
	}

	#renderDomains() {
		return html` ${repeat(
			this._domains,
			(domain) => domain,
			(domain, index) => {
				const options = this.#languageModel.map((model) => ({
					name: model.name,
					value: model.isoCode,
					selected: domain.isoCode ? domain.isoCode === model.isoCode : model.isDefault,
				}));
				return html`<uui-button-group class="domain">
					<uui-input
						label=${this.localize.term('assignDomain_domain')}
						value=${domain.domainName}
						@change=${(e: UUIInputEvent) => this.#changeDomainName(e, index)}></uui-input>
					<uui-select
						label=${this.localize.term('assignDomain_language')}
						.options=${options}
						@change=${(e: UUISelectEvent) => this.#changeIsoCode(e, index)}></uui-select>
					<uui-button look="outline" color="danger" label=${this.localize.term('assignDomain_remove')} compact>
						<uui-icon name="icon-trash" @click=${() => this.#remove(index)}></uui-icon>
					</uui-button>
				</uui-button-group>`;
			},
		)}`;
	}

	#renderAddNewDomainButton() {
		return html`<uui-button-group>
			<uui-button
				label=${this.localize.term('assignDomain_addNew')}
				look="placeholder"
				@click=${() => this.#addDomain()}></uui-button>
			<uui-button
				id="dropdown"
				label=${this.localize.term('buttons_select')}
				look="placeholder"
				compact
				@click=${() => this.#addDomain(true)}>
				<uui-icon name="icon-navigation-down"></uui-icon>
			</uui-button>
		</uui-button-group>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-button-group {
				width: 100%;
			}

			uui-box:first-child {
				margin-bottom: var(--uui-size-layout-1);
			}

			#dropdown {
				flex-grow: 0;
			}

			uui-select {
				display: grid;
			}

			.domain {
				display: grid;
				margin-bottom: var(--uui-size-2);
				grid-template-columns: 1fr 1fr auto;
				background-color: var(--uui-interface-surface-alt);
			}
		`,
	];
}

export default UmbCultureAndHostnamesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-culture-and-hostnames-modal': UmbCultureAndHostnamesModalElement;
	}
}
