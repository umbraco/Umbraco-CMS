import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils/index.js';
import { TemplatingSectionType } from '../../types.js';
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
	/*
	@queryAll('umb-insert-section-checkbox')
	checkboxes!: NodeListOf<UmbInsertSectionCheckboxElement>;

	@state()
	selectedCheckbox?: UmbInsertSectionCheckboxElement | null = null;

	@state()
	snippet = '';
*/
	/*
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
	*/

	/*
	firstUpdated() {
		this.selectedCheckbox = this.checkboxes[0];
	}
	*/

	//snippetMethods = [getRenderBodySnippet, getRenderSectionSnippet, getAddSectionSnippet];

	@state()
	private _pickedSection?: TemplatingSectionType;

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		/*
		const value = this.selectedCheckbox?.snippet;
		if (this.selectedCheckbox?.validate()) {
			this.value = { value: value ?? '' };
			this.modalContext?.submit();
		}
		*/
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<uui-box>
					<div id="main">
						${this.#renderRenderChildTemplate()} ${this.#renderRenderANamedSection()}
						${this.#renderDefineANamedSection()}
					</div>
				</uui-box>

				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label="Close">Close</uui-button>
					<uui-button @click=${this.#submit} look="primary" color="positive" label="Submit">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderRenderChildTemplate() {
		return html`<uui-button
			label=${this.localize.term('template_renderBody')}
			@click=${() => (this._pickedSection = TemplatingSectionType.renderChildTemplate)}
			look="placeholder">
			${this._pickedSection === TemplatingSectionType.renderChildTemplate
				? html`<uui-badge color="positive"><uui-icon name="icon-check"></uui-icon></uui-badge>`
				: ''}
			<h3><umb-localize key="template_renderBody">Render Child Template</umb-localize></h3>
			<p>
				<umb-localize key="template_renderBodyDesc">
					Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.
				</umb-localize>
			</p>
		</uui-button>`;
	}

	#renderRenderANamedSection() {
		return html`<uui-button
			label=${this.localize.term('template_renderSection')}
			@click=${() => (this._pickedSection = TemplatingSectionType.renderANamedSection)}
			look="placeholder">
			${this._pickedSection === TemplatingSectionType.renderANamedSection
				? html`<uui-badge color="positive"><uui-icon name="icon-check"></uui-icon></uui-badge>`
				: ''}
			<h3><umb-localize key="template_renderSection">Render a named section</umb-localize></h3>
			<p>
				<umb-localize key="template_renderSectionDesc">
					Renders a named area of a child template, by inserting a
					<code>@RenderSection(name)</code> placeholder. This renders an area of a child template which is wrapped in a
					corresponding <code>@section [name]{ ... }</code> definition.
				</umb-localize>
			</p>
			${this._pickedSection === TemplatingSectionType.renderANamedSection
				? html`<div class="section">
						<uui-label for="section-name" required>Section Name</uui-label>
						<uui-input id="section-name" label="section name"></uui-input>
						<uui-checkbox label=${this.localize.term('template_sectionMandatory')}></uui-checkbox>
						<small>
							<umb-localize key="template_sectionMandatoryDesc">
								If mandatory, the child template must contain a <code>@section</code> definition, otherwise an error is
								shown.
							</umb-localize>
						</small>
				  </div>`
				: ''}
		</uui-button>`;
	}

	#renderDefineANamedSection() {
		return html`<uui-button
			label=${this.localize.term('template_defineSection')}
			@click=${() => (this._pickedSection = TemplatingSectionType.defineANamedSection)}
			look="placeholder">
			${this._pickedSection === TemplatingSectionType.defineANamedSection
				? html`<uui-badge color="positive"><uui-icon name="icon-check"></uui-icon></uui-badge>`
				: ''}
			<h3><umb-localize key="template_defineSection">Define a named section</umb-localize></h3>
			<p>
				<umb-localize key="template_defineSectionDesc">
					Defines a part of your template as a named section by wrapping it in <code>@section { ... }</code>. This can
					be rendered in a specific area of the parent of this template, by using <code>@RenderSection</code>.
				</umb-localize>
			</p>
		</uui-button>`;
	}

	/*
	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<div id="main">
					<uui-box>
						<uui-button label="Render Child Template">
							<uui-badge color="positive"><uui-icon name="icon-check"></uui-icon></uui-badge>
							<h3>Render Child Template</h3>
							Renders the contents of a child template, by inserting a @RenderBody() placeholder.
						</uui-button>
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
	*/

	static styles = [
		UmbTextStyles,
		css`
			/*
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
			*/

			code {
				background-color: var(--uui-color-surface-alt);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}

			#main {
				display: grid;
				grid-gap: var(--uui-size-space-5);
			}

			.section {
				display: grid;
			}

			uui-button {
				text-align: left;
			}

			uui-button p {
				margin-top: 0;
			}

			uui-input,
			small {
				margin-block: var(--uui-size-space-2) var(--uui-size-space-6);
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
