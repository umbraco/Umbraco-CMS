import {LitElement} from "lit";
import {customElement, property, state} from "lit/decorators.js";
import {until} from "lit/directives/until.js";
import {loadCustomView, renderCustomView} from "../utils/load-custom-view.function.js";

@customElement('umb-custom-view')
export default class UmbCustomViewElement extends LitElement {
  @property({ attribute: 'custom-view' })
  set customView (view: string) {
    this.#loadView(view);
  }

  @property({ attribute: 'args' })
  args?: any;

  @state()
  protected component: any = null;

  async #loadView(view: string) {
    if (!view || !view.endsWith('.js') && !view.endsWith('.html')) {
      return;
    }

    const customView = await loadCustomView(view);

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
