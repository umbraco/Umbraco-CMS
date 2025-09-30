import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils/index.js';
import { TemplatingSectionType } from '../../types.js';
import type {
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue,
} from './templating-section-picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIBooleanInputElement, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-templating-section-picker-modal')
export class UmbTemplatingSectionPickerModalElement extends UmbModalBaseElement<
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue
> {
	@query('#render-named-section-name')
	private _renderNamedSectionNameInput?: UUIInputElement;

	@query('#define-named-section-name')
	private _defineNamedSectionNameInput?: UUIInputElement;

	@query('#render-named-section-is-mandatory')
	private _renderNamedSectionIsMandatoryCheckbox?: UUIBooleanInputElement;

	@state()
	private _pickedSection: TemplatingSectionType = TemplatingSectionType.renderChildTemplate;

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		switch (this._pickedSection) {
			case TemplatingSectionType.renderChildTemplate:
				this.value = { value: getRenderBodySnippet() };
				break;
			case TemplatingSectionType.renderANamedSection:
				this.value = {
					value: getRenderSectionSnippet(
						this._renderNamedSectionNameInput?.value as string,
						this._renderNamedSectionIsMandatoryCheckbox?.checked ?? false,
					),
				};
				break;
			case TemplatingSectionType.defineANamedSection:
				this.value = { value: getAddSectionSnippet(this._defineNamedSectionNameInput?.value as string) };
				break;
		}
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<uui-box>
					<div id="main">
						${this.#renderRenderChildTemplate()} ${this.#renderRenderANamedSection()}
						${this.#renderDefineANamedSection()}
					</div>
				</uui-box>

				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label=${this.localize.term('general_close')}></uui-button>
					<uui-button
						@click=${this.#submit}
						look="primary"
						color="positive"
						label=${this.localize.term('general_submit')}></uui-button>
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
						<uui-label for="render-named-section-name" required>
							<umb-localize key="template_sectionName">Section Name</umb-localize>
						</uui-label>
						<uui-input id="render-named-section-name" label=${this.localize.term('template_sectionName')}></uui-input>
						<uui-checkbox
							id="render-named-section-is-mandatory"
							label=${this.localize.term('template_sectionMandatory')}></uui-checkbox>
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
			${this._pickedSection === TemplatingSectionType.defineANamedSection
				? html`<div class="section">
						<uui-label for="define-named-section-name" required>
							<umb-localize key="template_sectionName">Section Name</umb-localize>
						</uui-label>
						<uui-input id="define-named-section-name" label=${this.localize.term('template_sectionName')}></uui-input>
					</div>`
				: ''}
		</uui-button>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
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
