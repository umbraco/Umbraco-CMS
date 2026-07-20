import {ApiHelpers} from "./ApiHelpers";

export class SmtpApiHelper {
  api: ApiHelpers;
  private smtpBaseUrl = 'http://localhost:5000'; // Default smtp4dev URL, can be configured

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAllEmails() {
    const response = await this.api.page.request.get(this.smtpBaseUrl + '/api/messages', {
      ignoreHTTPSErrors: true
    });
    return await response.json();
  }

  async deleteAllEmails() {
    const response = await this.api.page.request.delete(this.smtpBaseUrl + '/api/messages/*', {
      ignoreHTTPSErrors: true
    });
    return response.status();
  }

  async findEmailBySubject(subject: string) {
    const emails = await this.getAllEmails();
    const foundEmail = emails.results.find((email: any) =>
      email.subject && email.subject.toLowerCase().includes(subject.toLowerCase())
    );
    return foundEmail || null;
  }

  async findEmailToRecipient(recipientEmail: string) {
    const emails = await this.getAllEmails();
    const target = recipientEmail.toLowerCase();
    const foundEmail = emails.results.find((email: any) => {
      const recipients = Array.isArray(email.to) ? email.to.join(',') : email.to;
      return typeof recipients === 'string' && recipients.toLowerCase().includes(target);
    });
    return foundEmail || null;
  }

  async getEmailHtmlById(id: string): Promise<string> {
    const response = await this.api.page.request.get(this.smtpBaseUrl + '/api/messages/' + id + '/html', {
      ignoreHTTPSErrors: true
    });
    return await response.text();
  }

  async extractPasswordResetUrlForRecipient(recipientEmail: string): Promise<string | null> {
    const email = await this.findEmailToRecipient(recipientEmail);
    if (!email) {
      return null;
    }
    const html = await this.getEmailHtmlById(email.id);
    // The reset URL contains: flow=reset-password&userId=...&resetCode=...
    const match = html.match(/https?:\/\/[^"'\s<>]+flow=reset-password[^"'\s<>]*/);
    return match ? match[0] : null;
  }

  async doesNotificationEmailWithSubjectExist(actionName: string, contentName: string) {
    const expectedSubject = `Notification about ${actionName} performed on ${contentName}`
    return this.findEmailBySubject(expectedSubject);
  }
}
