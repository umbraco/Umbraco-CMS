import { html, customElement, state, css, query, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	CultureAndHostnames,
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-culture-and-hostnames-modal')
export class UmbCultureAndHostnamesModalElement extends UmbModalBaseElement<
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue
> {
	#unique?: string | null;

	@state()
	private _cultureOptions: Array<Option> = [{ name: 'Inherit', value: 'inherit', selected: true }];

	@state()
	private _domains: Array<CultureAndHostnames> = [];

	constructor() {
		super();
		this.#getCultureAndDomains();
	}

	async #getCultureAndDomains() {
		// TODO get this documents culture and domains data via repository
	}

	firstUpdated() {
		this.#unique = this.data?.unique;
	}

	connectedCallback(): void {
		super.connectedCallback();
		if (!this.modalContext) return;

		this.observe(this.modalContext.value, (value) => {
			if (value) this._domains = value.data;
		});
	}

	#handleCancel() {
		this.modalContext?.reject();
	}
	#handleSave() {
		// TODO validation before submitting
		this.modalContext?.submit();
	}

	#addDomain(currentDomain?: boolean) {
		if (currentDomain) {
			this._domains = [...this._domains, { culture: 'en-us', hostname: window.location.host }];
		} else {
			this._domains = [...this._domains, { culture: 'en-us', hostname: '' }];
		}

		this.value = { data: this._domains };
	}

	#remove(index: number) {
		this._domains = this._domains.filter((_, i) => i !== index);
	}

	#changeHostname(e: UUIInputEvent, index: number) {
		this._domains[index] = { ...this._domains[index], hostname: e.target.value as string };
	}

	#changeCulture(e: UUISelectEvent, index: number) {
		this._domains[index] = { ...this._domains[index], culture: e.target.value as string };
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_assigndomain')}>
				<uui-box>
					<h2>Culture</h2>
					<uui-label for="select">${this.localize.term('assignDomain_language')}</uui-label>
					<uui-select
						id="select"
						label=${this.localize.term('assignDomain_language')}
						.options=${this._cultureOptions}></uui-select>

					<h2>Domains</h2>
					<p>
						<umb-localize key="assignDomain_domainHelpWithVariants">
							Valid domain names are: "example.com", "www.example.com", "example.com:8080", or
							"https://www.example.com/".<br />Furthermore also one-level paths in domains are supported, eg.
							"example.com/en" or "/en".
						</umb-localize>
					</p>
					${this.#renderDomains()}
					<uui-button-group>
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
					</uui-button-group>
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
			(domain, index) => domain.hostname + domain.culture + index,
			(domain, index) =>
				html`<uui-button-group class="domain">
					<uui-input
						label=${this.localize.term('assignDomain_domain')}
						value=${domain.hostname}
						@change=${(e: UUIInputEvent) => this.#changeHostname(e, index)}></uui-input>
					<uui-select
						label=${this.localize.term('assignDomain_language')}
						.options=${this._cultureOptions}></uui-select>
					<uui-button look="outline" color="danger" label=${this.localize.term('assignDomain_remove')} compact>
						<uui-icon name="icon-trash" @click=${() => this.#remove(index)}></uui-icon>
					</uui-button>
				</uui-button-group>`,
		)}`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-button-group {
				width: 100%;
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
