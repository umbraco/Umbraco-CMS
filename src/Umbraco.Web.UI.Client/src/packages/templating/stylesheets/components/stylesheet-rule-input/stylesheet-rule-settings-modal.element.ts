import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-stylesheet-rule-settings-modal')
export default class UmbStylesheetRuleSettingsModalElement extends UmbModalBaseElement<never, any> {
	#updateName(event: Event) {
		const name = (event.target as HTMLInputElement).value;

		this.updateValue({
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			rule: {
				...this.value.rule,
				name,
			},
		});
	}

	#updateSelector(event: Event) {
		const selector = (event.target as HTMLInputElement).value;

		this.updateValue({
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			rule: {
				...this.value.rule,
				selector,
			},
		});
	}

	#updateStyles(event: Event) {
		const styles = (event.target as HTMLInputElement).value;

		this.updateValue({
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			rule: {
				...this.value.rule,
				styles,
			},
		});
	}

	override render() {
		return html`
			<umb-body-layout headline="Edit style">
				<div id="main">
					<uui-box>
						<uui-form>
							<form id="MyForm" name="myForm">
								<uui-form-layout-item>
									<uui-label for="name" slot="label" required>Name</uui-label>
									<span slot="description">The name displayed in the editor style selector</span>
									<uui-input
										id="name"
										name="name"
										.value=${this.value.rule?.name ?? ''}
										label="Rule name"
										required
										@input=${this.#updateName}>
									</uui-input>
								</uui-form-layout-item>
								<uui-form-layout-item>
									<uui-label for="selector" slot="label" required>Selector</uui-label>
									<span slot="description">Uses CSS syntax, e.g. "h1" or ".redHeader"</span>
									<uui-input
										id="selector"
										name="selector"
										.value=${this.value.rule?.selector ?? ''}
										label="Rule selector"
										@input=${this.#updateSelector}
										required>
									</uui-input>
								</uui-form-layout-item>
								<uui-form-layout-item>
									<uui-label for="styles" slot="label">Styles</uui-label>
									<span slot="description"
										>The CSS that should be applied in the rich text editor, e.g. "color:red;"</span
									>
									<uui-textarea
										@input=${this.#updateStyles}
										id="styles"
										name="styles"
										.value=${this.value.rule?.styles ?? ''}
										label="Rule styles">
									</uui-textarea>
								</uui-form-layout-item>
								<uui-form-layout-item>
									<uui-label for="styles" slot="label">Preview</uui-label>
									<span slot="description">How the text will look like in the rich text editor.</span>
									<div style="${ifDefined(this.value.rule?.styles)}">
										a b c d e f g h i j k l m n o p q r s t u v w x t z
										<br />
										A B C D E F G H I J K L M N O P Q R S T U V W X Y Z
										<br />
										1 2 3 4 5 6 7 8 9 0 € £ $ % &amp; (.,;:'"!?)
										<br />
										Just keep examining every bid quoted for zinc etchings.
									</div>
								</uui-form-layout-item>
							</form>
						</uui-form>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._rejectModal} look="secondary" label="Close">Close</uui-button>
					<uui-button @click=${this._submitModal} look="primary" color="positive" label="Submit">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
			}

			#main {
				box-sizing: border-box;
				height: calc(
					100dvh - var(--umb-header-layout-height) - var(--umb-footer-layout-height) - 2 * var(--uui-size-layout-1)
				);
			}

			#main uui-button:not(:last-of-type) {
				display: block;
				margin-bottom: var(--uui-size-space-5);
			}

			uui-input {
				width: 100%;
			}

			#styles {
				font-family:
					Monaco,
					Menlo,
					Consolas,
					Courier New,
					monospace;
				--uui-textarea-min-height: 100px;
				resize: none;
				width: 300px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-rule-settings-modal': UmbStylesheetRuleSettingsModalElement;
	}
}
