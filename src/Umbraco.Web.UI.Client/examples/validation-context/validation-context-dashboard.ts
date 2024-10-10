import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_CONTEXT, umbBindToValidation, UmbValidationContext } from '@umbraco-cms/backoffice/validation';

@customElement('umb-example-validation-context-dashboard')
export class UmbExampleValidationContextDashboard extends UmbLitElement {

	readonly validation = new UmbValidationContext(this);

	@state()
	name = 'Lorem Ipsum'

	@state()
	email = 'lorem.ipsum@test.com'

	@state()
  messages? : any[]

	/**
	 *
	 */
	constructor() {
		super();

		this.consumeContext(UMB_VALIDATION_CONTEXT,(validationContext)=>{
      console.log('validation ctx',validationContext);

      this.observe(validationContext.messages.messages,(messages)=>{
        this.messages = messages;
      },'observeValidationMessages')


    });
	}

	async #handleValidateNow() {

		await this.validation.validate().catch(()=>{});

		console.log('Valid', this.validation.isValid);

	}

	#handleAddServerValidationError() {
		this.validation.messages.addMessage('server','$.name','Name server-error message','4875e113-cd0c-4c57-ac92-53d677ba31ec');
		this.validation.messages.addMessage('server','$.email','Email server-error message','4a9e5219-2dbd-4ce3-8a79-014eb6b12428');
	}

	override render() {
		return html`
			<uui-box>
				This is a element with a Validation Context.

				<uui-form>
					<form>
						<div>
						<label>Name</label>
						<uui-form-validation-message>
							<uui-input
								type="text"
								.value=${this.name}
								@input=${(e: InputEvent)=>this.name = (e.target as HTMLInputElement).value}
								${umbBindToValidation(this,'$.name',this.name)}
								required></uui-input>
						</uui-form-validation-message>
						</div>
						<label>E-mail</label>
						<uui-form-validation-message>
							<uui-input
								type="email"
								.value=${this.email}
								@input=${(e: InputEvent)=>this.email = (e.target as HTMLInputElement).value}
								${umbBindToValidation(this,'$.email',this.email)}
								required></uui-input>
						</uui-form-validation-message>
					</form>
				</uui-form>
				<button @click=${this.#handleValidateNow} title="Click to trigger validation on the validation context">Validate now</button>
				<button @click=${this.#handleAddServerValidationError} title="">Add server error</button>

				<hr/>
				<pre>${JSON.stringify(this.messages ?? [],null,3)}</pre>


			</uui-box>
		`
	}

	static override styles = [css`

		label {
			display:block;
		}

    uui-box {
      margin:20px;
    }

		pre {
      text-align:left;
      padding:10px;
      border:1px dotted #6f6f6f;
      background: #f2f2f2;
      font-size: 11px;
      line-height: 1.3em;
    }

  `]

}

export default UmbExampleValidationContextDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-validation-context-dashboard': UmbExampleValidationContextDashboard;
	}
}
