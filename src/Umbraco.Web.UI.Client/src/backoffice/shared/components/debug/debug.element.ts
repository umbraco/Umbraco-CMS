import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextDebugRequest, UmbContextRequestEventImplementation, umbContextRequestEventType } from '@umbraco-cms/context-api';


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
				height:auto;
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

	@property()
	contextAliases = new Map();

    @state()
	private _debugPaneOpen = false;

    private _toggleDebugPane() {
        this._debugPaneOpen = !this._debugPaneOpen;
    }

	connectedCallback(): void {
		super.connectedCallback();
		
		// Dispatch it
		this.dispatchEvent(new UmbContextDebugRequest((instances)=> {
			console.log('I have contexts now', instances);

			this.contextAliases = instances;
		}));
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
								${this._renderContextAliases()}
							</ul>
						</div>						
					</div>
                </div>
			`;
        }

        return nothing;
	}

	private _renderContextAliases() {
		const aliases = [];

		for (const [alias, instance] of this.contextAliases) {
			aliases.push(
				html`
					<li>
						${alias}
						<ul>
							${this._renderInstance(instance)}
						</ul>
					</li>`
				);
		}

		return aliases;
	}

	private _renderInstance(instance: any) {
		const instanceKeys = [];

		for(const key in instance) {
			// Goes KABOOM - if try to loop over the class/object
			// instanceKeys.push(html`<li>${key} = ${instance[key]}</li>`);

			instanceKeys.push(html`<li>${key}</li>`);
		}

		return instanceKeys;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-debug': UmbDebug;
	}
}
