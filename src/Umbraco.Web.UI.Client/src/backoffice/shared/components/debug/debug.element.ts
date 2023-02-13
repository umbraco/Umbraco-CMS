import { UmbContextRequestEventImplementation, umbContextRequestEventType } from '@umbraco-cms/context-api';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { styleMap } from 'lit-html/directives/style-map';
import { customElement, property, state } from 'lit/decorators.js';


@customElement('umb-debug')
export class UmbDebug extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
			}
            
            #debug {
                display: block;
				font-family: monospace;
                /* background:red; */

                position:absolute;
                bottom:0;
                left:0;
                z-index:10000;
                width:calc(100% - 20px);

                padding:10px;
            }

            #debug:hover {
                /* background-color:blue; */
            }

            .events {
                background-color:var(--uui-color-danger);
                color:var(--uui-color-selected-contrast);
                height:0;
                transition: height 0.3s ease-out;
            }

            .events.open {
                height:200px;
                padding:10px;
            }

            h4 { 
                margin:0;
            }

		`,
	];

    @property({reflect: true, type: Boolean})
    enabled = false;

    @state()
	private _debugPaneOpen = false;

    private _toggleDebugPane() {
        this._debugPaneOpen = !this._debugPaneOpen;
    }

    constructor() {
        super();

        // Get outer element <umb-app>
        const app = window.document.querySelector('umb-app');

        console.log('root app', app);
        app?.addEventListener(umbContextRequestEventType as unknown as UmbContextRequestEventImplementation, (e) => {
            // Console.log event
            console.log('Some event', e.type);
            console.log('Some event thing', e);
            console.log('Some event thing', e.contextAlias);

        });

        //this.addEventListener('click', (e) => console.log(e.type, e.target.localName));
    }

	render() {

        if(this.enabled){
            return html`
                <div id="debug">
                    <uui-button color="danger" look="primary" @click="${this._toggleDebugPane}">Debug</uui-button>
                    <div class="events ${this._debugPaneOpen ? 'open' : ''}">
                        <h4>Events</h4>
                        
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
