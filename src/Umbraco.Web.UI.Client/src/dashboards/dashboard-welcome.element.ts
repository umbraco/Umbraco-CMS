import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

@defineElement('umb-dashboard-welcome')
export class UmbDashboardWelcome extends LitElement {
  static styles = [UUITextStyles, css``];

  render() {
    return html` <div>
      <h1>POC Introduction</h1>
      <p>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris lectus turpis, facilisis in quam non, mattis
        varius massa. Nullam pretium, dui in facilisis sollicitudin, diam tortor scelerisque tellus, eget cursus ex orci
        et mauris. Aliquam condimentum enim a erat ultricies blandit. Aliquam urna sem, lacinia ut eros id, placerat
        commodo libero. Pellentesque iaculis lacus in neque pharetra dignissim. Duis gravida nibh in consequat lobortis.
        Nullam sollicitudin ante orci, ac molestie lectus maximus non.
      </p>
      <ul>
        <li>Nulla luctus, neque sed feugiat laoreet</li>
        <li>Nam varius orci velit, quis interdum enim facilisis sed.</li>
      </ul>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-dashboard-welcome': UmbDashboardWelcome;
  }
}
