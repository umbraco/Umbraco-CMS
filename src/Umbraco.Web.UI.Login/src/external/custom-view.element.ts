import {LitElement} from "lit";
import {customElement, property, state} from "lit/decorators.js";
import {until} from "lit/directives/until.js";
import {loadCustomView, renderCustomView} from "../utils/load-custom-view.function.js";

@customElement('umb-custom-view')
export default class UmbCustomViewElement extends LitElement {
  @property({ attribute: 'custom-view' })
  customView?: string;

  @property({ attribute: 'args' })
  args?: any;

  @state()
  protected component: any = null;

  attributeChangedCallback(name: string, _old: string | null, value: string | null) {
    super.attributeChangedCallback(name, _old, value);
    if (name === 'custom-view') {
      this.#loadView();
    }
  }

  async #loadView() {
    if (!this.customView || !this.customView.endsWith('.js') && !this.customView.endsWith('.html')) {
      return;
    }

    const customView = await loadCustomView(this.customView);

    if (this.args) {
      Object.entries(this.args).forEach(([key, value]) => {
        (customView as any)[key] = value;
      });
    }

    this.component = renderCustomView(customView);
  }

  render() {
    return until(this.component);
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-custom-view': UmbCustomViewElement;
  }
}
