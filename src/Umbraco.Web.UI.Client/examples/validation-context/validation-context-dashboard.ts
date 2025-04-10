import { html, customElement, css, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_CONTEXT, umbBindToValidation, UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import type { UmbValidationMessage } from 'src/packages/core/validation/context/validation-messages.manager';

@customElement('umb-example-validation-context-dashboard')
export class UmbExampleValidationContextDashboardElement extends UmbLitElement {
	readonly validation = new UmbValidationContext(this);

	@state()
	name = '';

	@state()
	email = '';

	@state()
	city = '';

	@state()
	country = '';

	@state()
	messages?: UmbValidationMessage[];

	@state()
	totalErrorCount = 0;

	@state()
	tab1ErrorCount = 0;

	@state()
	tab2ErrorCount = 0;

	@state()
	tab = '1';

	constructor() {
		super();

		this.consumeContext(UMB_VALIDATION_CONTEXT, (validationContext) => {
			this.observe(
				validationContext.messages.messages,
				(messages) => {
					this.messages = messages;
				},
				'observeValidationMessages',
			);

			// Observe all errors
			this.validation.messages.messagesOfPathAndDescendant('$.form').subscribe((value) => {
				this.totalErrorCount = [...new Set(value.map((x) => x.path))].length;
			});

			// Observe errors for tab1, note that we only use part of the full JSONPath
			this.validation.messages.messagesOfPathAndDescendant('$.form.tab1').subscribe((value) => {
				this.tab1ErrorCount = [...new Set(value.map((x) => x.path))].length;
			});

			// Observe errors for tab2, note that we only use part of the full JSONPath
			this.validation.messages.messagesOfPathAndDescendant('$.form.tab2').subscribe((value) => {
				this.tab2ErrorCount = [...new Set(value.map((x) => x.path))].length;
			});
		});
	}

	#onTabChange(e: Event) {
		this.tab = (e.target as HTMLElement).getAttribute('data-tab') as string;
	}

	#handleSave() {
		// fake server validation-errors for all fields
		if (this.name == '')
			this.validation.messages.addMessage(
				'server',
				'$.form.tab1.name',
				'Name server-error message',
				'4875e113-cd0c-4c57-ac92-53d677ba31ec',
			);
		if (this.email == '')
			this.validation.messages.addMessage(
				'server',
				'$.form.tab1.email',
				'Email server-error message',
				'a47e287b-4ce6-4e8b-8e05-614e7cec1a2a',
			);
		if (this.city == '')
			this.validation.messages.addMessage(
				'server',
				'$.form.tab2.city',
				'City server-error message',
				'8dfc2f15-fb9a-463b-bcec-2c5d3ba2d07d',
			);
		if (this.country == '')
			this.validation.messages.addMessage(
				'server',
				'$.form.tab2.country',
				'Country server-error message',
				'd98624f6-82a2-4e94-822a-776b44b01495',
			);
	}

	override render() {
		return html`
			<uui-box>
				This is a demo of how the Validation Context can be used to validate a form with multiple steps. Start typing in
				the form or press Save to trigger validation.
				<hr />
				Total errors: ${this.totalErrorCount}
				<hr />
				<uui-tab-group @click=${this.#onTabChange}>
					<uui-tab ?active=${this.tab == '1'} data-tab="1">
						Tab 1
						${when(this.tab1ErrorCount, () => html` <uui-badge color="invalid">${this.tab1ErrorCount}</uui-badge> `)}
					</uui-tab>
					<uui-tab ?active=${this.tab == '2'} data-tab="2">
						Tab 2
						${when(this.tab2ErrorCount, () => html` <uui-badge color="invalid">${this.tab2ErrorCount}</uui-badge> `)}
					</uui-tab>
				</uui-tab-group>

				${when(this.tab == '1', () => html` ${this.#renderTab1()} `)}
				${when(this.tab == '2', () => html` ${this.#renderTab2()} `)}

				<uui-button look="primary" color="positive" @click=${this.#handleSave}>Save</uui-button>
				<hr />
				<h3>Validation Context Messages</h3>
				<pre>${JSON.stringify(this.messages ?? [], null, 3)}</pre>
			</uui-box>
		`;
	}

	#renderTab1() {
		return html`
			<uui-form>
				<form>
					<div>
						<label>Name</label>
						<uui-form-validation-message>
							<uui-input
								type="text"
								.value=${this.name}
								@input=${(e: InputEvent) => (this.name = (e.target as HTMLInputElement).value)}
								${umbBindToValidation(this, '$.form.tab1.name', this.name)}
								required></uui-input>
						</uui-form-validation-message>
					</div>
					<label>E-mail</label>
					<uui-form-validation-message>
						<uui-input
							type="email"
							.value=${this.email}
							@input=${(e: InputEvent) => (this.email = (e.target as HTMLInputElement).value)}
							${umbBindToValidation(this, '$.form.tab1.email', this.email)}
							required></uui-input>
					</uui-form-validation-message>
				</form>
			</uui-form>
		`;
	}

	#renderTab2() {
		return html`
			<uui-form>
				<form>
					<div>
						<label>City</label>
						<uui-form-validation-message>
							<uui-input
								type="text"
								.value=${this.city}
								@input=${(e: InputEvent) => (this.city = (e.target as HTMLInputElement).value)}
								${umbBindToValidation(this, '$.form.tab2.city', this.city)}
								required></uui-input>
						</uui-form-validation-message>
					</div>
					<label>Country</label>
					<uui-form-validation-message>
						<uui-input
							type="text"
							.value=${this.country}
							@input=${(e: InputEvent) => (this.country = (e.target as HTMLInputElement).value)}
							${umbBindToValidation(this, '$.form.tab2.country', this.country)}
							required></uui-input>
					</uui-form-validation-message>
				</form>
			</uui-form>
		`;
	}

	static override styles = [
		css`
			uui-badge {
				top: 0;
				right: 0;
				font-size: 10px;
				min-width: 17px;
				min-height: 17px;
			}

			label {
				display: block;
			}

			uui-box {
				margin: 20px;
			}

			uui-button {
				margin-top: 1rem;
			}

			pre {
				text-align: left;
				padding: 10px;
				border: 1px dotted #6f6f6f;
				background: #f2f2f2;
				font-size: 11px;
				line-height: 1.3em;
			}
		`,
	];
}

export default UmbExampleValidationContextDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-validation-context-dashboard': UmbExampleValidationContextDashboardElement;
	}
}
