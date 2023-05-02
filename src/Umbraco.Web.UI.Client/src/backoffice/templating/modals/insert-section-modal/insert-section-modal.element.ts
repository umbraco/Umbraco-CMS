import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS } from '../manifests';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

import './insert-section-input.element';

export const UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL = new UmbModalToken(
	UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

export interface InsertSectionModalModalResult {
	value?: string;
}

@customElement('umb-templating-insert-section-modal')
export default class UmbTemplatingInsertSectionModalElement extends UmbModalBaseElement<
	object,
	InsertSectionModalModalResult
> {
	#chooseSection() {
		this.modalHandler?.submit({ value: 'test' });
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box>
						<umb-insert-section-checkbox label="Render child template">
							<p>Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox label="Render a named section">
							<p>
								Renders a named area of a child template, by inserting a <code>@RenderSection(name)</code> placeholder.
								This renders an area of a child template which is wrapped in a corresponding
								<code>@section [name]{ ... }</code> definition.
							</p>
							<uui-form-layout-item slot="if-checked">
								<uui-label slot="label">Section name</uui-label>
								<uui-input placeholder="Enter section name"></uui-input>
							</uui-form-layout-item>
							<p slot="if-checked">
								<uui-checkbox>Section is mandatory </uui-checkbox><br />
								<small
									>If mandatory, the child template must contain a <code>@section</code> definition, otherwise an error
									is shown.</small
								>
							</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox label="Define a named section">
							<p>
								Defines a part of your template as a named section by wrapping it in <code>@section { ... }</code>. This
								can be rendered in a specific area of the parent of this template, by using <code>@RenderSection</code>.
							</p>
							<uui-form-layout-item slot="if-checked">
								<uui-label slot="label">Section name</uui-label>
								<uui-input placeholder="Enter section name"></uui-input>
							</uui-form-layout-item>
						</umb-insert-section-checkbox>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main umb-insert-section-checkbox:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-section-modal': UmbTemplatingInsertSectionModalElement;
	}
}
