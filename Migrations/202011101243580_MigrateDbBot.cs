namespace Telegram_bot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDbBot : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CountMessages",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        UserId = c.Int(nullable: false),
                        Counter = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Rates",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        UserId = c.Int(nullable: false),
                        rate = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Rates");
            DropTable("dbo.CountMessages");
        }
    }
}
