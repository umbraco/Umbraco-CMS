import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils/index.js';
import type {
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue,
} from './templating-section-picker-modal.token.js';
import type { UmbInsertSectionCheckboxElement } from './templating-section-picker-input.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import './templating-section-picker-input.element.js';

@customElement('umb-templating-section-picker-modal')
export class UmbTemplatingSectionPickerModalElement extends UmbModalBaseElement<
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue
> {
	@queryAll('umb-insert-section-checkbox')
	checkboxes!: NodeListOf<UmbInsertSectionCheckboxElement>;

	@state()
	selectedCheckbox?: UmbInsertSectionCheckboxElement | null = null;

	@state()
	snippet = '';

	#chooseSection(event: Event) {
		const target = event.target as UmbInsertSectionCheckboxElement;
		const checkboxes = Array.from(this.checkboxes);
		if (checkboxes.every((checkbox) => checkbox.checked === false)) {
			this.selectedCheckbox = null;
			return;
		}
		if (target.checked) {
			this.selectedCheckbox = target;
			this.snippet = this.selectedCheckbox.snippet ?? '';
			checkboxes.forEach((checkbox) => {
				if (checkbox !== target) {
					checkbox.checked = false;
				}
			});
		}
	}

	firstUpdated() {
		this.selectedCheckbox = this.checkboxes[0];
	}

	snippetMethods = [getRenderBodySnippet, getRenderSectionSnippet, getAddSectionSnippet];

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		const value = this.selectedCheckbox?.snippet;
		if (this.selectedCheckbox?.validate()) {
			this.value = { value: value ?? '' };
			this.modalContext?.submit();
		}
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<div id="main">
					<uui-box>
						<umb-insert-section-checkbox
							@change=${this.#chooseSection}
							label=${this.localize.term('template_renderBody')}
							checked
							.snippetMethod=${getRenderBodySnippet}>
							<p slot="description">
								<umb-localize key="template_renderBodyDesc">
									Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.
								</umb-localize>
							</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox
							@change=${this.#chooseSection}
							label=${this.localize.term('template_renderSection')}
							show-mandatory
							show-input
							.snippetMethod=${getRenderSectionSnippet}>
							<p slot="description">
								<umb-localize key="template_renderSectionDesc">
									Renders a named area of a child template, by inserting a
									<code>@RenderSection(name)</code> placeholder. This renders an area of a child template which is
									wrapped in a corresponding <code>@section [name]{ ... }</code> definition.
								</umb-localize>
							</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox
							@change=${this.#chooseSection}
							label=${this.localize.term('template_defineSection')}
							show-input
							.snippetMethod=${getAddSectionSnippet}>
							<p slot="description">
								<umb-localize key="template_defineSectionDesc">
									Renders a named area of a child template, by inserting a
									<code>@RenderSection(name)</code> placeholder. This renders an area of a child template which is
									wrapped in a corresponding <code>@section [name]{ ... }</code> definition.
								</umb-localize>
							</p>
						</umb-insert-section-checkbox>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label="Close">Close</uui-button>
					<uui-button @click=${this.#submit} look="primary" color="positive" label="Submit">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
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

			#main umb-insert-section-checkbox:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}
			code {
				background-color: var(--uui-color-surface-alt);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}
		`,
	];
}

export default UmbTemplatingSectionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-section-picker-modal': UmbTemplatingSectionPickerModalElement;
	}
}
