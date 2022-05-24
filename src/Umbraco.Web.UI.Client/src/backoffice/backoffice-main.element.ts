import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';

import { UmbContextInjectMixin } from '../core/context';
import { UmbSectionContext } from '../section.context';

import { UmbExtensionManifest, UmbManifestSectionMeta } from '../core/extension';

// TODO: lazy load these. How to we handle dynamic import of our typescript file?
import '../content/content-section.element';
import '../media/media-section.element';

@defineElement('umb-backoffice-main')
export class UmbBackofficeMain extends UmbContextInjectMixin(LitElement) {
  static styles = [
    UUITextStyles,
    css`
      :host {
        flex: 1 1 auto;
      }

      // TODO: not call this editor, I would like to reserve that name for the view of editing data, like what goes in the router-outlet or in infinite editors (or inside Nested Content)
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
        flex: none;
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
        display: flex;
        flex: 1;
        flex-direction: column;
        gap: 16px;
      }

      uui-tab {
        font-size: 0.8rem;
      }

      #editor-bottom {
        display: flex;
        flex: none;
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

  @state()
  private _sectionElement?: HTMLElement;

  private _sectionContext?: UmbSectionContext;
  private _currentSectionSubscription?: Subscription;

  connectedCallback(): void {
    super.connectedCallback();
    this.requestContext('umbRouter');
    this.requestContext('umbSectionContext');
  }

  contextInjected(contexts: Map<string, any>): void {
    if (contexts.has('umbSectionContext')) {
      this._sectionContext = contexts.get('umbSectionContext');
      this._useCurrentSection();
    }
  }

  private _useCurrentSection () {
    this._currentSectionSubscription?.unsubscribe();

    this._currentSectionSubscription = this._sectionContext?.getCurrent()
    .subscribe(section => {
      this._createSectionElement(section);
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._currentSectionSubscription?.unsubscribe();
  }

  private async _createSectionElement (section: UmbExtensionManifest<UmbManifestSectionMeta>) {
    if (!section) return;

    // TODO: How do we handle dynamic imports of our files?
    if (section.js) {
      await import(/* @vite-ignore */section.js);
    }

    if (section.elementName) {
      this._sectionElement = document.createElement(section.elementName);
    }
  }

  render() {
    return html`
      ${ this._sectionElement }
      <!--
      <div id="editor">
        <div id="editor-top">
          <uui-input value="Home"></uui-input>
          <uui-tab-group>
            <uui-tab active>Content</uui-tab>
            <uui-tab>Info</uui-tab>
            <uui-tab disabled>Actions</uui-tab>
          </uui-tab-group>
        </div>
        <uui-scroll-container id="editor-content"></uui-scroll-container>
        <div id="editor-bottom">
          <uui-button>Save and preview</uui-button>
          <uui-button look="secondary">Save</uui-button>
          <uui-button look="primary" color="positive">Save and publish</uui-button>
        </div>
      </div>
      -->
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-backoffice-main': UmbBackofficeMain;
  }
}
