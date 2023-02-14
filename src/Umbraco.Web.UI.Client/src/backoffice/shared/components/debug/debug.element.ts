import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextRequestEventImplementation, umbContextRequestEventType } from '@umbraco-cms/context-api';


@customElement('umb-debug')
export class UmbDebug extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			#container {
				display: block;
				font-family: monospace;

				z-index:10000;

				width:100%;
				padding:10px 0;
			}

			.events {
				background-color:var(--uui-color-danger);
				color:var(--uui-color-selected-contrast);
				height:0;
				transition: height 0.3s ease-out;
			}

			.events.open {
				height:200px;
			}

			.events > div {
				padding:10px;
			}

			h4 {
				margin:0;
			}
		`,
	];

    @property({reflect: true, type: Boolean})
    enabled = false;

	@property({type: Array<String>})
	contextAliases = ['UmbTemplateDetailStore', 'umbLanguageStore'];

    @state()
	private _debugPaneOpen = false;

    private _toggleDebugPane() {
        this._debugPaneOpen = !this._debugPaneOpen;
    }

	connectedCallback(): void {
		super.connectedCallback();

		// Create event that bubbles up through the DOM
		const event = new CustomEvent('umb:debug-contexts', {
			bubbles: true,
			composed: true			
		});

		// Dispatch it
		this.dispatchEvent(event);
	}

	render() {

        if(this.enabled){
            return html`
                <div id="container">
					<uui-button color="danger" look="primary"  @click="${this._toggleDebugPane}">
						<uui-icon name="umb:bug"></uui-icon>
						Debug
					</uui-button>

					<div class="events ${this._debugPaneOpen ? 'open' : ''}">
						<div>
							<h4>Context Aliases to consume</h4>
							<ul>
								${this.contextAliases.map((ctxAlias) =>
									html`<li>${ctxAlias}</li>`
								)}
							</ul>
						</div>						
					</div>
                </div>
			`;
        }

        return nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-debug': UmbDebug;
	}
}
