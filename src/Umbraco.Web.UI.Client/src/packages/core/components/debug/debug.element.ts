import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing, TemplateResult } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import {
	contextData,
	DebugContextData,
	DebugContextItemData,
	UmbContextDebugRequest,
} from '@umbraco-cms/backoffice/context-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext, UMB_CONTEXT_DEBUGGER_MODAL, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

@customElement('umb-debug')
export class UmbDebugElement extends UmbLitElement {
	@property({ reflect: true, type: Boolean })
	visible = false;

	@property({ reflect: true, type: Boolean })
	dialog = false;

	@state()
	contextData = Array<DebugContextData>();

	@state()
	private _debugPaneOpen = false;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		// Dispatch it
		this.dispatchEvent(
			new UmbContextDebugRequest((contexts: Map<any, any>) => {
				// The Contexts are collected
				// When travelling up through the DOM from this element
				// to the root of <umb-app> which then uses the callback prop
				// of the this event tha has been raised to assign the contexts
				// back to this property of the WebComponent

				// Massage the data into a simplier array of objects
				// From a function in the context-api '
				this.contextData = contextData(contexts);
			})
		);
	}

	render() {
		if (this.visible) {
			return this.dialog ? this._renderDialog() : this._renderPanel();
		} else {
			return nothing;
		}
	}

	private _toggleDebugPane() {
		this._debugPaneOpen = !this._debugPaneOpen;
	}

	private _openDialog() {
		this._modalContext?.open(UMB_CONTEXT_DEBUGGER_MODAL, {
			content: html`${this._renderContextAliases()}`,
		});
	}

	private _renderDialog() {
		return html` <div id="container">
			<uui-badge color="danger" look="primary" attention @click="${this._openDialog}">
				<uui-icon name="umb:bug"></uui-icon> Debug
			</uui-badge>
		</div>`;
	}

	private _renderPanel() {
		return html` <div id="container">
			<uui-button color="danger" look="primary" @click="${this._toggleDebugPane}">
				<uui-icon name="umb:bug"></uui-icon>
				Debug
			</uui-button>

			<div class="events ${this._debugPaneOpen ? 'open' : ''}">
				<div>
					<ul>
						${this._renderContextAliases()}
					</ul>
				</div>
			</div>
		</div>`;
	}

	private _renderContextAliases() {
		const contextsTemplates: TemplateResult[] = [];

		this.contextData.forEach((contextData) => {
			contextsTemplates.push(
				html` <li>
					Context: <strong>${contextData.alias}</strong>
					<em>(${contextData.type})</em>
					<ul>
						${this._renderInstance(contextData.data)}
					</ul>
				</li>`
			);
		});

		return contextsTemplates;
	}

	private _renderInstance(instance: DebugContextItemData) {
		const instanceTemplates: TemplateResult[] = [];

		if (instance.type === 'function') {
			return instanceTemplates.push(html`<li>Callable Function</li>`);
		} else if (instance.type === 'object') {
			if (instance.methods?.length) {
				instanceTemplates.push(
					html`
						<li>
							<strong>Methods</strong>
							<ul>
								${instance.methods?.map((methodName) => html`<li>${methodName}</li>`)}
							</ul>
						</li>
					`
				);
			}

			const props: TemplateResult[] = [];
			instance.properties?.forEach((property) => {
				switch (property.type) {
					case 'string':
					case 'number':
					case 'boolean':
					case 'object':
						props.push(html`<li>${property.key} <em>(${property.type})</em> = ${property.value}</li>`);
						break;

					default:
						props.push(html`<li>${property.key} <em>(${property.type})</em></li>`);
						break;
				}
			});

			instanceTemplates.push(html`
				<li>
					<strong>Properties</strong>
					<ul>
						${props}
					</ul>
				</li>
			`);
		} else if (instance.type === 'primitive') {
			instanceTemplates.push(html`<li>Context is a primitive with value: ${instance.value}</li>`);
		}

		return instanceTemplates;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				float: right;
			}

			#container {
				display: block;
				font-family: monospace;

				z-index: 10000;

				position: relative;
				width: 100%;
				padding: 10px 0;
			}

			uui-badge {
				cursor: pointer;
			}

			uui-icon {
				font-size: 15px;
			}

			.events {
				background-color: var(--uui-color-danger);
				color: var(--uui-color-selected-contrast);
				max-height: 0;
				transition: max-height 0.25s ease-out;
				overflow: hidden;
			}

			.events.open {
				max-height: 500px;
				overflow: auto;
			}

			.events > div {
				padding: 10px;
			}

			h4 {
				margin: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-debug': UmbDebugElement;
	}
}
