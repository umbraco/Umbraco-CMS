import { css, CSSResultGroup, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, state } from 'lit/decorators.js';
@customElement('umb-installer-installing')
export class UmbInstallerInstalling extends LitElement {
  static styles: CSSResultGroup = [
    css`
      h1 {
        text-align: center;
      }
    `,
  ];

  @state()
  private _installProgress = 0;

  protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
    super.firstUpdated(_changedProperties);

    this._updateProgress();
  }

  private async _updateProgress() {
    this._installProgress = Math.min(this._installProgress + (Math.random() + 1) * 10, 100);
    await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000));
    console.log('progress', this._installProgress);

    if (this._installProgress >= 100) {
      // Redirect to backoffice
      history.pushState(null, '', '/backoffice/backoffice');
      return;
    }

    this._updateProgress();
  }

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Installing Umbraco</h1>
      <uui-progress-bar progress=${this._installProgress}></uui-progress-bar>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-installing': UmbInstallerInstalling;
  }
}
