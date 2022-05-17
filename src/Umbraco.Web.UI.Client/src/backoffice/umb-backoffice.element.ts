import { LitElement, html, css } from 'lit';
import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import './umb-backoffice-header.element';

@defineElement('umb-backoffice')
export class UmbBackoffice extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: flex;
        flex-direction: column;
        min-height: 100%;
        width: 100%;
        color: var(--uui-color-text);
        font-size: 14px;
        box-sizing: border-box;
      }

      #nav-top-bar {
        display: flex;
        color: rgb(230, 230, 230);
        gap: 24px;
        padding: 0 var(--uui-size-4);
        align-items: center;
        background-color: var(--uui-color-header);
        height: 48px;
        width: 100%;
        font-size: 1rem;
        box-sizing: border-box;
        --uui-tab-text: white;
        --uui-tab-text-active: var(--uui-color-current);
        --uui-tab-text-hover: var(--uui-color-current-emphasis);
      }
      #main {
        width: 100%;
        height: calc(100% - 48px);
        display: flex;
        box-sizing: border-box;
      }
      #nav-side-bar {
        width: 400px;
        background-color: var(--uui-color-surface);
        height: 100%;
        border-right: 1px solid var(--uui-color-border);
        font-weight: 500;
        display: flex;
        flex-direction: column;
      }
      #nav-side-bar b {
        padding: var(--uui-size-6) var(--uui-size-8);
      }
      #editor {
        background-color: var(--uui-color-background);
        width: 100%;
        height: 100%;
        display: flex;
        flex-direction: column;
      }
      #editor-top {
        background-color: var(--uui-color-surface);
        width: 100%;
        display: flex;
        gap: 16px;
        align-items: center;
        border-bottom: 1px solid var(--uui-color-border);
      }
      #editor-top uui-input {
        width: 100%;
        margin-left: 16px;
      }
      #editor-top uui-tab-group {
        --uui-tab-divider: var(--uui-color-border);
        border-left: 1px solid var(--uui-color-border);
        flex-wrap: nowrap;
        height: 60px;
      }
      #editor-content {
        padding: var(--uui-size-6);
        height: 100%;
        display: flex;
        flex-direction: column;
        gap: 16px;
      }
      .editor-property {
        display: grid;
        grid-template-columns: 200px 600px;
        gap: 32px;
      }
      .editor-property uui-input,
      .editor-property uui-textarea {
        width: 100%;
      }

      uui-box hr {
        margin-bottom: var(--uui-size-6);
      }

      hr {
        border: 0;
        border-top: 1px solid var(--uui-color-border-alt);
      }

      uui-tab {
        font-size: 0.8rem;
      }

      #editor-bottom {
        display: flex;
        justify-content: end;
        align-items: center;
        height: 70px;
        width: 100%;
        gap: 16px;
        padding-right: 24px;
        border-top: 1px solid var(--uui-color-border);
        background-color: var(--uui-color-surface);
        box-sizing: border-box;
      }
    `,
  ];

  render() {
    return html`
      <umb-backoffice-header></umb-backoffice-header>
      <div id="main">
        <div id="nav-side-bar">
          <b>Content</b>
          <div class="nav-list">
            <uui-menu-item label="Hello World">
              <uui-icon slot="icon" name="document"></uui-icon>
            </uui-menu-item>
            <uui-menu-item label="Home" active has-children show-children>
              <uui-icon slot="icon" name="document"></uui-icon>
              <uui-menu-item label="Products">
                <uui-icon slot="icon" name="document"></uui-icon>
              </uui-menu-item>
              <uui-menu-item label="People">
                <uui-icon slot="icon" name="document"></uui-icon>
              </uui-menu-item>
              <uui-menu-item label="About Us" disabled has-children>
                <uui-icon slot="icon" name="document"></uui-icon>
                <uui-menu-item label="History">
                  <uui-icon slot="icon" name="document"></uui-icon>
                </uui-menu-item>
                <uui-menu-item label="Team">
                  <uui-icon slot="icon" name="document"></uui-icon>
                </uui-menu-item>
              </uui-menu-item>
              <uui-menu-item label="MyMenuItem" selected has-children>
                <uui-icon slot="icon" name="document"></uui-icon>
                <uui-menu-item label="History">
                  <uui-icon slot="icon" name="document"></uui-icon>
                </uui-menu-item>
                <uui-menu-item label="Team">
                  <uui-icon slot="icon" name="document"></uui-icon>
                </uui-menu-item>
              </uui-menu-item>
              <uui-menu-item label="Blog">
                <uui-icon slot="icon" name="calendar"></uui-icon>
              </uui-menu-item>
              <uui-menu-item label="Contact"></uui-menu-item>
            </uui-menu-item>
            <uui-menu-item label="Recycle Bin">
              <uui-icon slot="icon" name="delete"></uui-icon>
            </uui-menu-item>
          </div>
        </div>
        <div id="editor">
          <div id="editor-top">
            <uui-input value="Home"></uui-input>
            <uui-tab-group>
              <uui-tab active>Content</uui-tab>
              <uui-tab>Info</uui-tab>
              <uui-tab disabled>Actions</uui-tab>
            </uui-tab-group>
          </div>
          <uui-scroll-container id="editor-content">
            <uui-box>
              <div slot="headline">Hero</div>
              <div class="editor-property">
                <div class="label">
                  <b>Header</b>
                  <p>This is the main headline for the hero area on the Homepage</p>
                </div>
                <div class="value">
                  <uui-input type="text" value="You have typed some stuff into this one already"></uui-input>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Description</b>
                  <p>Write something nice</p>
                </div>
                <div class="value">
                  <uui-textarea>I should probably write some more</uui-textarea>
                  <div class="warning-box">
                    Warning, the operation you're about to perform might influence other part of
                    <b>Project Name</b>. as well as other projects
                  </div>
                </div>
              </div>
            </uui-box>
            <uui-box>
              <div slot="headline">Options</div>
              <div class="editor-property">
                <div class="label">
                  <b>Disabled</b>
                  <p>This is the main headline for the hero area on the Homepage</p>
                </div>
                <div class="value">
                  <uui-input value="this is disabled" disabled></uui-input>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Header</b>
                  <p>This is the main headline for the hero area on the Homepage</p>
                </div>
                <div class="value">
                  <uui-input></uui-input>
                  <div class="danger-box">
                    Warning, the operation you're about to perform might influence other part of
                    <b>Project Name</b>. as well as other projects
                  </div>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Options</b>
                  <p>This will show some options</p>
                </div>
                <div class="value">
                  <uui-combobox-list>
                    <uui-combobox-list-option> Option 1 </uui-combobox-list-option>
                    <uui-combobox-list-option> Option 2 </uui-combobox-list-option>
                    <uui-combobox-list-option> Option 3 </uui-combobox-list-option>
                    <uui-combobox-list-option> Option 4 </uui-combobox-list-option>
                    <uui-combobox-list-option> Option 5 </uui-combobox-list-option>
                  </uui-combobox-list>
                </div>
              </div>
            </uui-box>
            <uui-box>
              <div slot="headline">Buttons</div>
              <div class="editor-property">
                <div class="label">
                  <b>Primary</b>
                </div>
                <div class="value">
                  <div class="buttons">
                    <uui-button color="primary">Button</uui-button>
                    <uui-button color="primary" look="primary"> Button </uui-button>
                    <uui-button color="primary" look="secondary"> Button </uui-button>
                    <uui-button color="primary" look="outline"> Button </uui-button>
                    <uui-button color="primary" look="placeholder"> Button </uui-button>
                  </div>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Positive</b>
                </div>
                <div class="value">
                  <div class="buttons">
                    <uui-button color="positive">Button</uui-button>
                    <uui-button color="positive" look="primary"> Button </uui-button>
                    <uui-button color="positive" look="secondary"> Button </uui-button>
                    <uui-button color="positive" look="outline"> Button </uui-button>
                    <uui-button color="positive" look="placeholder"> Button </uui-button>
                  </div>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Warning</b>
                </div>
                <div class="value">
                  <div class="buttons">
                    <uui-button color="warning">Button</uui-button>
                    <uui-button color="warning" look="primary"> Button </uui-button>
                    <uui-button color="warning" look="secondary"> Button </uui-button>
                    <uui-button color="warning" look="outline"> Button </uui-button>
                    <uui-button color="warning" look="placeholder"> Button </uui-button>
                  </div>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Danger</b>
                </div>
                <div class="value">
                  <div class="buttons">
                    <uui-button color="danger">Button</uui-button>
                    <uui-button color="danger" look="primary"> Button </uui-button>
                    <uui-button color="danger" look="secondary"> Button </uui-button>
                    <uui-button color="danger" look="outline"> Button </uui-button>
                    <uui-button color="danger" look="placeholder"> Button </uui-button>
                  </div>
                </div>
              </div>
              <hr />
              <div class="editor-property">
                <div class="label">
                  <b>Disabled</b>
                </div>
                <div class="value">
                  <div class="buttons">
                    <uui-button disabled color="danger">Button</uui-button>
                    <uui-button disabled color="danger" look="primary"> Button </uui-button>
                    <uui-button disabled color="danger" look="secondary"> Button </uui-button>
                    <uui-button disabled color="danger" look="outline"> Button </uui-button>
                    <uui-button disabled color="danger" look="placeholder"> Button </uui-button>
                  </div>
                </div>
              </div>
            </uui-box>
          </uui-scroll-container>
          <div id="editor-bottom">
            <uui-button>Save and preview</uui-button>
            <uui-button look="secondary">Save</uui-button>
            <uui-button look="primary" color="positive">Save and publish</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice': UmbBackoffice;
  }
}
